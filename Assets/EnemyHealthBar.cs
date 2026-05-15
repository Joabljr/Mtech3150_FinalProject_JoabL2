using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider slider;

    public void SetMax(int max)
    {
        slider.maxValue = max;
        slider.value = max;
    }

    public void SetValue(int value)
    {
        slider.value = value;
    }
}
