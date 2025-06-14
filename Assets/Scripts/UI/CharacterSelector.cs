using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    public GameObject characterImage;
    private Animator animator;

    private void Start()
    {
        animator = characterImage.GetComponent<Animator>();
    }

    public void SelectCharacter(int index)
    {
        animator.SetInteger("Index", index);
    }
}