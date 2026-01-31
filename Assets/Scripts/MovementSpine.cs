using UnityEngine;
using UnityEngine.InputSystem;
using Spine;
using Spine.Unity;

public class MovementSpine : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    
    [Header("Spine Animations - Directional")]

    [SerializeField] private Transform textureHolder;
    [SerializeField] private Animator frontAnimator;
    [SerializeField] private Animator backAnimator;
    [SerializeField] private Animator sideAnimator;

    private string actionName = "action";
    private string hitName = "hit";
    
    private Animator currentAnimator;

    void Start()
    {
        SetActiveAnimation(frontAnimator);
    }

    void Update()
    {
        Vector3 directionToCamera = Camera.main.transform.position - textureHolder.position;
        directionToCamera.y = 0;
        textureHolder.rotation = Quaternion.LookRotation(directionToCamera);

        Vector2 moveInput = GameManager.instance.inputActions.Player.Move.ReadValue<Vector2>();

        if (GameManager.instance.isCableEnjoyerChosen)
        {
            frontAnimator.SetBool("walk", moveInput.magnitude > 0);
            transform.Translate(new Vector3(moveInput.x, 0, moveInput.y) * speed * Time.deltaTime);
            UpdateAnimationDirection(moveInput);
        }
    }
    
    void UpdateAnimationDirection(Vector2 moveInput)
    {
        // if (moveInput.magnitude < 0.1f || moveInput.y < -0.9f)
        // {
        //     SetActiveAnimation(frontAnimator);
        // }
        // else if ()
        // Determine which direction is dominant
        if (Mathf.Abs(moveInput.y) > Mathf.Abs(moveInput.x))
        {
            // Vertical movement is dominant
            if (moveInput.y > 0)
            {
                // Moving up - show back
                SetActiveAnimation(backAnimator);
            }
            else
            {
                // Moving down - show front
                SetActiveAnimation(frontAnimator);
            }
        }
        else
        {
            SetActiveAnimation(sideAnimator);
            sideAnimator.transform.localScale = new Vector3(moveInput.x > 0 ? -1 : 1, 1, 1);
        }
    }
    
    void SetActiveAnimation(Animator animator)
    {
        var animState = frontAnimator.GetAnimatorTransitionInfo(0);
        if (animState.IsName(hitName) || animState.IsName(actionName))
        {
            frontAnimator.gameObject.SetActive(false);
            backAnimator.gameObject.SetActive(false);
            sideAnimator.gameObject.SetActive(false);
            
            currentAnimator = frontAnimator;
            currentAnimator.gameObject.SetActive(true);
        }
        // if (animator == currentAnimator || 
        frontAnimator.gameObject.SetActive(false);
        backAnimator.gameObject.SetActive(false);
        sideAnimator.gameObject.SetActive(false);
        
        currentAnimator = animator;
        currentAnimator.gameObject.SetActive(true);
    }

    public void HandleDangerCollision(Danger danger)
    {
        SetActiveAnimation(frontAnimator);
        Debug.Log($"Collided with danger of type: {danger.Type} on circuit: {danger.LightCircuit}");
    }
}
