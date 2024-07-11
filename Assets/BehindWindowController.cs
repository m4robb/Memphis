using UnityEngine;
using DG.Tweening;

public class BehindWindowController : MonoBehaviour
{
    public Renderer ScrimRenderer;
    public GameObject GOBehindWindow;
    void Start()
    {
        GOBehindWindow.SetActive(false);
        ScrimRenderer.material.DOFade(1, 0);
    }

    private bool IsFading = false;
    
    public void FadeScrimOut()
    {
        if (IsFading) return;
        IsFading = true;
        GOBehindWindow.SetActive(true);
        ScrimRenderer.enabled = true;
        ScrimRenderer.material.DOFade(0.5f, 2f).OnComplete(() =>
        {
            IsFading = false;
        });
    }
    public void FadeScrimIn()
    {
        if (IsFading) return;
        IsFading = true;
        ScrimRenderer.material.DOFade(1, 2f).OnComplete(() =>
        {
            IsFading = false;
            ScrimRenderer.enabled = false;
            GOBehindWindow.SetActive(false);
        });
    }
    

}
