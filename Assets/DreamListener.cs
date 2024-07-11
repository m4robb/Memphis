using UnityEngine;



public class DreamListener : MonoBehaviour
{

    public LineRenderer LM0;
    public LineRenderer LM1;
    public LineRenderer LM2;
    public LineRenderer LM3;
    public LineRenderer LM4;

    public Color C0;
    public Color C1;
    public Color C2;
    public Color C3;
    public Color C4;

    Material M0;
    Material M1;
    Material M2;
    Material M3;
    Material M4;


    private void Start()
    {
        M0 = LM0.material;
        M1 = LM1.material;
        M2 = LM2.material;
        M3 = LM3.material;
        M4 = LM4.material;
    }
    void Update()
    {
        float[] spectrum = new float[64];
        float[] spectrumGrouped = new float[5];
        float updateTime = 0f;

        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        for (int i = 0; i < 1; i++ ) spectrumGrouped[0] += spectrum[i];
        for (int i = 2; i < 5; i++) spectrumGrouped[1] += spectrum[i];
        for (int i = 6; i < 13; i++) spectrumGrouped[2] += spectrum[i];
        for (int i = 14; i < 30; i++) spectrumGrouped[3] += spectrum[i];
        for (int i = 31; i < 63; i++) spectrumGrouped[4] += spectrum[i];

        C0.a = spectrumGrouped[0];
        M0.color = C0;


        C1.a = spectrumGrouped[1];
        M1.color = C1;

        C2.a = spectrumGrouped[2];
        M2.color = C2;


        C3.a = spectrumGrouped[3];
        M3.color = C3;


        C4.a = spectrumGrouped[4];
        M4.color = C4;
    }
}