using UnityEngine;
using UnityEngine.InputSystem;
using Spine;
using Spine.Unity;

public class MovementSpine : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    
    [Header("Spine Animations - Directional")]
    public SkeletonAnimation frontAnimation;  // PRZÓD - moving down
    public SkeletonAnimation backAnimation;   // TYŁ - moving up
    public SkeletonAnimation sideAnimation;   // BOK - moving left/right
    
    [Header("Animation Names")]
    public string idleAnimationName = "idle";
    public string walkAnimationName = "walk";
    
    private SkeletonAnimation currentAnimation;
    private bool isMoving = false;
    private Vector2 lastMoveDirection = Vector2.down; // Start facing front

    void Start()
    {
        // Start with front animation
        SetActiveAnimation(frontAnimation);
        if (currentAnimation != null)
        {
            PlayAnimation(idleAnimationName, true);
        }
    }

    void Update()
    {
        Vector2 moveInput = Vector2.zero;

        if(GameManger.instance.useFirstMap)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
                if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
                if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
                if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
            }
        
            // Move the player
            if (moveInput != Vector2.zero)
            {
                transform.Translate(new Vector3(moveInput.x, 0, moveInput.y) * speed * Time.deltaTime);
                
                // Update last move direction
                lastMoveDirection = moveInput.normalized;
                
                // Switch animation based on movement direction
                UpdateAnimationDirection(moveInput);
                
                // Play walk animation
                if (!isMoving)
                {
                    PlayAnimation(walkAnimationName, true);
                    isMoving = true;
                }
            }
            else
            {
                // Play idle animation
                if (isMoving)
                {
                    PlayAnimation(idleAnimationName, true);
                    isMoving = false;
                }
            }
        }
    }
    
    void UpdateAnimationDirection(Vector2 moveInput)
    {
        // Determine which direction is dominant
        if (Mathf.Abs(moveInput.y) > Mathf.Abs(moveInput.x))
        {
            // Vertical movement is dominant
            if (moveInput.y > 0)
            {
                // Moving up - show back
                SetActiveAnimation(backAnimation);
            }
            else
            {
                // Moving down - show front
                SetActiveAnimation(frontAnimation);
            }
        }
        else
        {
            // Horizontal movement is dominant - show side
            SetActiveAnimation(sideAnimation);
            
            // Flip for left/right
            if (sideAnimation != null && sideAnimation.Skeleton != null)
            {
                sideAnimation.Skeleton.ScaleX = moveInput.x > 0 ? 1 : -1;
            }
        }
    }
    
    void SetActiveAnimation(SkeletonAnimation newAnimation)
    {
        if (newAnimation == null || newAnimation == currentAnimation) return;
        
        // Disable all animations
        if (frontAnimation != null) frontAnimation.gameObject.SetActive(false);
        if (backAnimation != null) backAnimation.gameObject.SetActive(false);
        if (sideAnimation != null) sideAnimation.gameObject.SetActive(false);
        
        // Enable the new animation
        currentAnimation = newAnimation;
        currentAnimation.gameObject.SetActive(true);
    }
    
    public void PlayAnimation(string animationName, bool loop = true, int trackIndex = 0)
    {
        if (currentAnimation == null || currentAnimation.AnimationState == null) return;
        
        TrackEntry trackEntry = currentAnimation.AnimationState.SetAnimation(trackIndex, animationName, loop);
        if (trackEntry == null)
        {
            Debug.LogWarning($"Animation '{animationName}' not found!");
        }
    }
    
    public void ChangeSkin(string skinName)
    {
        if (currentAnimation == null || currentAnimation.Skeleton == null) return;
        
        Skin newSkin = currentAnimation.Skeleton.Data.FindSkin(skinName);
        if (newSkin != null)
        {
            currentAnimation.Skeleton.SetSkin(newSkin);
            currentAnimation.Skeleton.SetSlotsToSetupPose();
            currentAnimation.Update(0);
        }
        else
        {
            Debug.LogWarning($"Skin '{skinName}' not found!");
        }
    }
}
