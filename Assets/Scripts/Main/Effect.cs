using UnityEngine;
using UnityEngine.UI;

public class Effect : MonoBehaviour
{
    [SerializeField]
    E_EFFECT_TYPE type;

    [SerializeField]
    bool isRazer;

    [SerializeField]
    Sprite[] effectSprites;

    Animator animator;
    
    void Awake()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    public void StartNormalEffect(Transform pos)
    {
        if (isRazer)
        {
            int num = Random.Range(0, effectSprites.Length);
            GetComponent<Image>().sprite = effectSprites[num];
        }
        gameObject.transform.localPosition = pos.localPosition;
        animator.SetTrigger("doNormal");
    }

    public void StartFeverEffect(Transform pos)
    {
        if (isRazer)
        {
            int num = Random.Range(0, effectSprites.Length);
            GetComponent<Image>().sprite = effectSprites[num];
        }
        gameObject.transform.localPosition = pos.localPosition;
        animator.SetTrigger("doFever");
    }

    public void EndEffect()
    {
        Destroy(this.gameObject);
    }
    
    public E_EFFECT_TYPE GetEffectType()
    {
        return type;
    }
}
