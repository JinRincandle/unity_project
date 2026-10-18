using UnityEngine;
using UnityEngine.InputSystem;
using Blocks.Character;
using Blocks.Movement;

namespace TimeTravel
{
    /// <summary>
    /// Comprehensive 2D Ladder Controller for Fractures of Time.
    /// Handles climbing up/down, auto-centering, platform surface transitions,
    /// descending from upper platforms, jump dismounting, and seamless walking.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class LadderClimb : MonoBehaviour
    {
        [Header("Climb Settings")]
        [SerializeField] private float climbSpeed = 5.0f;
        [SerializeField] private float horizontalClimbSpeed = 2.5f;
        [SerializeField] private float jumpDismountHeight = 2.2f;

        [Header("Platform Transition")]
        [Tooltip("Optional override for upper platform surface Y. If 0, auto-detected from geometry.")]
        [SerializeField] private float platformSurfaceYOverride = 0f;

        private BuildingBlocksCharacter character;
        private Rigidbody2D playerRb;
        private Transform playerTransform;
        private Collider2D playerCollider;
        private Animator playerAnimator;
        private Collider2D ladderCollider;

        private bool isClimbing = false;
        private float ladderTopY;
        private float ladderBottomY;
        private float ladderCenterX;
        private float platformTopY;
        private Collider2D upperPlatformCollider;
        private float exitCooldown = 0f;
        private float groundedTimer = 0f;

        // Player capsule constants (size=(0.99, 2.14), offset=(0.06, -0.61))
        private const float FootOffsetY = -1.68f;
        private const float HeadOffsetY = 0.46f;

        public bool IsClimbing => isClimbing;

        private void Awake()
        {
            ladderCollider = GetComponent<Collider2D>();
            if (ladderCollider != null) ladderCollider.isTrigger = true;
            CalculateBoundsAndPlatforms();
        }

        private void Start()
        {
            CalculateBoundsAndPlatforms();
        }

        public void CalculateBoundsAndPlatforms()
        {
            if (ladderCollider == null) ladderCollider = GetComponent<Collider2D>();
            if (ladderCollider == null) return;

            Bounds bounds = ladderCollider.bounds;
            ladderTopY = bounds.max.y;
            ladderBottomY = bounds.min.y;
            ladderCenterX = bounds.center.x;

            if (platformSurfaceYOverride != 0f)
            {
                platformTopY = platformSurfaceYOverride;
            }
            else
            {
                // Auto-detect upper platform surface near top of ladder
                platformTopY = ladderTopY - 1.8f; // Default assumption based on standard +1.8m trigger extension
                upperPlatformCollider = null;

                var hits = Physics2D.OverlapCircleAll(new Vector2(ladderCenterX, platformTopY), 1.6f);
                float bestSurface = float.MinValue;
                foreach (var h in hits)
                {
                    if (h == ladderCollider || h.isTrigger || h.CompareTag("Player") || h.GetComponentInParent<BuildingBlocksCharacter>() != null) continue;
                    if (h.bounds.max.y > bestSurface && h.bounds.max.y <= ladderTopY)
                    {
                        bestSurface = h.bounds.max.y;
                        upperPlatformCollider = h;
                    }
                }

                if (bestSurface > float.MinValue)
                {
                    platformTopY = bestSurface;
                }
            }
        }

        private void Update()
        {
            if (exitCooldown > 0f)
            {
                exitCooldown -= Time.deltaTime;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            CachePlayer(collision.gameObject);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (exitCooldown > 0f) return;

            if (character == null || playerRb == null)
            {
                CachePlayer(collision.gameObject);
                if (character == null || playerRb == null) return;
            }

            // Read Input (Supports Keyboard WASD / Arrows, Gamepad, and Input System)
            ReadInput(out float vertical, out float horizontal, out bool jumpPressed);

            float playerFeetY = playerRb.position.y + FootOffsetY;

            // 1. Not Climbing: Check if player initiates climbing
            if (!isClimbing)
            {
                bool wantsClimbUp = vertical > 0.25f;
                bool standingNearTop = Mathf.Abs(playerFeetY - platformTopY) < 0.6f || playerFeetY > platformTopY - 0.2f;
                bool wantsClimbDown = vertical < -0.25f && standingNearTop && Mathf.Abs(playerRb.position.x - ladderCenterX) < (ladderCollider.bounds.extents.x + 0.3f);

                if (wantsClimbUp || wantsClimbDown)
                {
                    StartClimbing(wantsClimbDown);
                }
                return;
            }

            // 2. Currently Climbing: Jump Dismount
            if (jumpPressed)
            {
                float facingDir = horizontal != 0f ? Mathf.Sign(horizontal) : character.FacingDirection;
                ExitLadder(false);
                character.Jump(jumpDismountHeight);
                playerRb.linearVelocity = new Vector2(facingDir * 3.5f, 6.0f);
                return;
            }

            // 3. Move Vertically & Center Horizontally
            float dt = Time.deltaTime;
            float targetX = ladderCenterX;

            // Allow slight horizontal steering if player holds A/D, but keep within rungs
            if (Mathf.Abs(horizontal) > 0.1f)
            {
                targetX += Mathf.Sign(horizontal) * 0.35f;
            }
            float newX = Mathf.MoveTowards(playerRb.position.x, targetX, horizontalClimbSpeed * dt);
            float newY = playerRb.position.y;

            if (Mathf.Abs(vertical) > 0.05f)
            {
                newY += vertical * climbSpeed * dt;
                groundedTimer = 0f;
            }
            else
            {
                // Idle on ladder: hold position firmly
                playerRb.linearVelocity = Vector2.zero;

                // Anti-stuck: if grounded on floor while idle, exit after a brief pause
                if (character.IsGrounded && playerFeetY <= platformTopY - 0.5f)
                {
                    groundedTimer += dt;
                    if (groundedTimer > 0.35f)
                    {
                        ExitLadder(false);
                        return;
                    }
                }
            }

            // Apply position
            Vector2 targetPos = new Vector2(newX, newY);
            playerRb.position = targetPos;
            playerTransform.position = new Vector3(targetPos.x, targetPos.y, playerTransform.position.z);
            playerRb.linearVelocity = Vector2.zero;

            // Update Animator
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("grounded", false);
                playerAnimator.SetFloat("velocityY", vertical * climbSpeed);
                playerAnimator.SetFloat("velocityX", 0f);
            }

            // 4. Platform Transitions
            // A) TOP OF LADDER: Transition onto Upper Platform
            if (playerFeetY >= platformTopY - 0.15f)
            {
                // If climbing up or pressing toward upper platform
                if (vertical > 0.05f || (playerFeetY >= platformTopY && Mathf.Abs(horizontal) > 0.1f))
                {
                    DismountOntoUpperPlatform(horizontal);
                    return;
                }
            }

            // B) BOTTOM OF LADDER: Transition onto Lower Ground
            if (playerFeetY <= ladderBottomY + 0.2f && vertical < 0f)
            {
                ExitLadder(false);
                return;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (playerTransform != null && (collision.transform == playerTransform || collision.transform.IsChildOf(playerTransform)))
            {
                if (isClimbing)
                {
                    ExitLadder(false);
                }
            }
        }

        private void StartClimbing(bool descendingFromTop)
        {
            isClimbing = true;
            exitCooldown = 0f;
            groundedTimer = 0f;

            if (character != null)
            {
                character.IsMovementEnabled = false;
            }

            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;

                if (descendingFromTop)
                {
                    // Snap slightly below platform surface to begin clean descent
                    float startY = platformTopY - FootOffsetY - 0.35f;
                    playerRb.position = new Vector2(ladderCenterX, startY);
                    playerTransform.position = new Vector3(ladderCenterX, startY, playerTransform.position.z);
                }
                else
                {
                    // Center on ladder horizontally
                    playerRb.position = new Vector2(ladderCenterX, playerRb.position.y);
                    playerTransform.position = new Vector3(ladderCenterX, playerTransform.position.y, playerTransform.position.z);
                }
            }

            // Temporarily ignore collision with upper platform while climbing through opening
            if (upperPlatformCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, upperPlatformCollider, true);
            }
        }

        private void DismountOntoUpperPlatform(float horizontalInput)
        {
            // Determine horizontal offset onto platform
            float stepDirection = horizontalInput != 0f ? Mathf.Sign(horizontalInput) : 1f;

            // If upper platform is predominantly to one side, step that way
            if (upperPlatformCollider != null)
            {
                if (upperPlatformCollider.bounds.center.x > ladderCenterX + 0.3f) stepDirection = 1f;
                else if (upperPlatformCollider.bounds.center.x < ladderCenterX - 0.3f) stepDirection = -1f;
            }

            float finalX = ladderCenterX + stepDirection * 0.85f;
            float finalY = platformTopY - FootOffsetY + 0.05f;

            playerRb.position = new Vector2(finalX, finalY);
            playerTransform.position = new Vector3(finalX, finalY, playerTransform.position.z);
            playerRb.linearVelocity = Vector2.zero;

            ExitLadder(true);
        }

        public void ExitLadder(bool reachedTop)
        {
            isClimbing = false;
            exitCooldown = 0.25f;
            groundedTimer = 0f;

            if (character != null)
            {
                character.IsMovementEnabled = true;
            }

            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }

            // Restore collision with upper platform
            if (upperPlatformCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, upperPlatformCollider, false);
            }

            if (playerAnimator != null)
            {
                playerAnimator.SetBool("grounded", true);
                playerAnimator.SetFloat("velocityY", 0f);
            }
        }

        private void CachePlayer(GameObject go)
        {
            var charComp = go.GetComponentInParent<BuildingBlocksCharacter>();
            var rb = go.GetComponentInParent<Rigidbody2D>();
            var col = go.GetComponentInParent<CapsuleCollider2D>() as Collider2D ?? go.GetComponentInParent<Collider2D>();
            var anim = go.GetComponentInChildren<Animator>() ?? go.GetComponentInParent<Animator>();

            if (charComp != null || rb != null || go.CompareTag("Player"))
            {
                character = charComp;
                playerRb = rb;
                playerCollider = col;
                playerAnimator = anim;
                playerTransform = charComp != null ? charComp.transform : (rb != null ? rb.transform : go.transform);
            }
        }

        private void ReadInput(out float vertical, out float horizontal, out bool jumpPressed)
        {
            vertical = 0f;
            horizontal = 0f;
            jumpPressed = false;

            // 1. New Input System Keyboard
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) vertical += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) vertical -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal += 1f;
                if (kb.spaceKey.wasPressedThisFrame) jumpPressed = true;
            }

            // 2. New Input System Gamepad
            var gp = Gamepad.current;
            if (gp != null)
            {
                Vector2 stick = gp.leftStick.ReadValue();
                if (Mathf.Abs(stick.y) > 0.2f) vertical = stick.y;
                if (Mathf.Abs(stick.x) > 0.2f) horizontal = stick.x;
                if (gp.dpad.up.isPressed) vertical = 1f;
                if (gp.dpad.down.isPressed) vertical = -1f;
                if (gp.dpad.left.isPressed) horizontal = -1f;
                if (gp.dpad.right.isPressed) horizontal = 1f;
                if (gp.buttonSouth.wasPressedThisFrame) jumpPressed = true;
            }
        }

        private void OnDisable()
        {
            if (isClimbing)
            {
                ExitLadder(false);
            }
        }

        private void OnDestroy()
        {
            if (isClimbing)
            {
                ExitLadder(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            CalculateBoundsAndPlatforms();

            Gizmos.color = new Color(0f, 1f, 0.4f, 0.8f);
            Gizmos.DrawLine(new Vector3(ladderCenterX - 1f, platformTopY, 0f), new Vector3(ladderCenterX + 1f, platformTopY, 0f));

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(ladderCenterX, platformTopY, 0f), 0.25f);
        }
    }
}
