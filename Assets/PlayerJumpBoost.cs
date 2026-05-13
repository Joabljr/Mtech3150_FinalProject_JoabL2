using UnityEngine;

public class PlayerJumpBoost : MonoBehaviour
{
    public float normalJumpHeight = 7f;
    private float currentJumpHeight;

    private void Start()
    {
        currentJumpHeight = normalJumpHeight;
    }

    public void ActivateJumpBoost(float boostHeight, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(JumpBoostRoutine(boostHeight, duration));
    }

    System.Collections.IEnumerator JumpBoostRoutine(float boostHeight, float duration)
    {
        currentJumpHeight = boostHeight;

        yield return new WaitForSeconds(duration);

        currentJumpHeight = normalJumpHeight;
    }

    public float GetJumpHeight()
    {
        return currentJumpHeight;
    }
}
