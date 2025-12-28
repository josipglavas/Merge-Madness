using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackgroundUI : MonoBehaviour
{
    [SerializeField] private RectTransform[] backgrounds;
    [SerializeField] private float speed = 100f;

    private float width;

   private void Start()
    {
        width = backgrounds[0].rect.width;
    }

    private void Update()
    {
        foreach (RectTransform bg in backgrounds)
        {
            bg.anchoredPosition += Vector2.left * (speed * Time.deltaTime);

            if (bg.anchoredPosition.x <= -width)
            {
                bg.anchoredPosition += Vector2.right * (width * backgrounds.Length);
            }
        }
    }
}