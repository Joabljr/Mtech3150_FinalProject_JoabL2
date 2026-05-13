using UnityEngine;


public class DayNightCycle : MonoBehaviour
{
    public float fullCycleDuration = 10;
    private float cycleProgress;
    private int cycle = 1;
    public int Day{get{return cycle;}}



    void Update()
    {
        if (cycleProgress < fullCycleDuration)
        {
            cycleProgress += Time.deltaTime;
        }
        else
        {
            cycle++;
            cycleProgress = 0;
        }

        transform.Rotate(Time.deltaTime / (fullCycleDuration / 360) , 0,0);
        
    }

        public float RemapRange(float oldValue, float oldMin, float oldMax, float newMin, float newMax)
    {
        float newValue = 0;
        float oldRange = (oldMax - oldMin);
        float newRange = (newMax - newMin);
        newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

        return newValue;
    }
}
