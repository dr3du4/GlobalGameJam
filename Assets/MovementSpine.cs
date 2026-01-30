using UnityEngine;
using UnityEngine.InputSystem;
using Spine;
using Spine.Unity;

public class MovementSpine : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    
    [Header("Spine Animation")]
    public SkeletonAnimation skeletonAnimation;
    public string idleAnimationName = "idle";
    public string walkAnimationName = "walk";
    
    private Skeleton skeleton;
    private Spine.AnimationState animationState;
    private bool isMoving = false;

    void Start()
    {
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
        }
        
        if (skeletonAnimation != null)
        {
            skeleton = skeletonAnimation.Skeleton;
            animationState = skeletonAnimation.AnimationState;
            
            PlayAnimation(idleAnimationName, true);
        }
    }

    void Update()
    {
        Vector2 moveInput = Vector2.zero;
        
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
            
            // Flip character based on movement direction
            if (skeleton != null && moveInput.x != 0)
            {
                skeleton.ScaleX = moveInput.x > 0 ? 1 : -1;
            }
            
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
    
    public void PlayAnimation(string animationName, bool loop = true, int trackIndex = 0)
    {
        if (animationState == null) return;
        
        TrackEntry trackEntry = animationState.SetAnimation(trackIndex, animationName, loop);
        if (trackEntry == null)
        {
            Debug.LogWarning($"Animation '{animationName}' not found!");
        }
    }
    
    public void ChangeSkin(string skinName)
    {
        if (skeleton == null) return;
        
        Skin newSkin = skeleton.Data.FindSkin(skinName);
        if (newSkin != null)
        {
            skeleton.SetSkin(newSkin);
            skeleton.SetSlotsToSetupPose();
            skeletonAnimation.Update(0);
        }
        else
        {
            Debug.LogWarning($"Skin '{skinName}' not found!");
        }
    }
}
