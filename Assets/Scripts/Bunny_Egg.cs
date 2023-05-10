using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BunnyType
{
    EasterEgg,
    BunnyWithHat,
    BabyBlob
}
public class Bunny_Egg : MonoBehaviour
{
    Animator animator;
    AudioSource soundFX;
    public int whackPoint, missPoint;
    public GameObject hitPrefab;
    public int requiredHits;
    int hits;
    public BunnyType bunnyType;
    public AudioClip whackSfx, missSfx;
    BoxCollider2D boxCollider2D;
    public float revealTime;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        soundFX = GetComponent<AudioSource>();
        whackPoint = GameplayManager.Instance.whackValue[bunnyType];
        missPoint = GameplayManager.Instance.missValue[bunnyType];
    }

    public void AnimHideTrigger() => Invoke("Hide", revealTime);
    void Hide()
    {
        if(bunnyType != BunnyType.BabyBlob) animator.SetTrigger("hide");
        else 
        {
            Instantiate(hitPrefab, transform.position, Quaternion.identity);
            animator.SetTrigger("hatch");
        }
    }

    public void HitSfx() => soundFX.PlayOneShot(whackSfx);
    public void MissSfx() => soundFX.PlayOneShot(missSfx);

    public void Whack()
    {
        if(!boxCollider2D.enabled)
            return;
        if(bunnyType != BunnyType.BabyBlob)
        {
            if(hits != requiredHits) hits++;
            if(hits == requiredHits) 
            {
                Instantiate(hitPrefab, transform.position, Quaternion.identity);
                animator.SetTrigger("hit");
            }
        }
        else animator.SetTrigger("hide");
    }

    public void GetPoint()
    {
        //Get the point
        if(GameplayManager.Instance.playerPoint >= 0) 
            GameplayManager.Instance.playerPoint += whackPoint;
        Debug.Log("Get Point");
        Destroy(gameObject);
    }

    public void Missed()
    {
        Debug.Log("Lost Point");
        //SubtractTime and missedPoint;
        if(GameplayManager.Instance.playerPoint >= 0) 
            GameplayManager.Instance.playerPoint -= missPoint;
        Destroy(gameObject);
    }
}
