using UnityEngine;

public class SmallEye : MonoBehaviour
{
   private Animator animator;

   public void HoldEye()
   {
      animator = GetComponent<Animator>();
      animator.SetTrigger("HoldEye");
   }
}
