using UnityEngine;
using UnityEngine.UI;

public class LaserWarningUI : MonoBehaviour
{
    public Image warningImage;

    public void Show()
    {
        warningImage.enabled = true;
    }

    public void Hide()
    {
        warningImage.enabled = false;
    }

    public void SetColor(Color c)
    {
        warningImage.color = c;
    }
}
