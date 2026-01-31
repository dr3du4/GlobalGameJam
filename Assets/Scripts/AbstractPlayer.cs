using UnityEngine;

public abstract class AbstractPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    
    
    [Header("Spine Animations - Directional")]

    [SerializeField] private Animator frontAnimator;
    [SerializeField] private Animator backAnimator;
    [SerializeField] private Animator sideAnimator;

    private string idleName = "idle";
    private string actionName = "action";
    private string hitName = "hit";
    
    private Animator currentAnimator;

    void Awake()
    {
        SetDefaultAnim();
    }

    protected void SetDefaultAnim()
    {
        SetActiveAnimation(frontAnimator);
    }

    void Update()
    {
        MyUpdate();
    }

    protected abstract void MyUpdate();
    protected void SetWalking(bool isWalking)
    {
        frontAnimator.SetBool("walk", isWalking);
    }
    
    protected void UpdateAnimationDirection(Vector2 moveDirection)
    {
        var animState = currentAnimator.GetCurrentAnimatorStateInfo(0);
        if (moveDirection.magnitude < 0.1f 
            || moveDirection.y < -0.9f 
            || animState.IsName(hitName) 
            || animState.IsName(actionName) 
            || animState.IsName(idleName)
        )
        {
            SetActiveAnimation(frontAnimator);
        }
        else if (Mathf.Abs(moveDirection.y) > Mathf.Abs(moveDirection.x))
        {
            SetActiveAnimation(backAnimator);
        }
        else
        {
            SetActiveAnimation(sideAnimator);
            sideAnimator.transform.localScale = new Vector3(moveDirection.x > 0 ? -1 : 1, 1, 1);
        }
    }
    
    protected void SetActiveAnimation(Animator animator)
    {
        if (animator == currentAnimator) return;

        frontAnimator.gameObject.SetActive(false);
        backAnimator.gameObject.SetActive(false);
        sideAnimator.gameObject.SetActive(false);
        
        currentAnimator = animator;
        currentAnimator.gameObject.SetActive(true);
    }
}
