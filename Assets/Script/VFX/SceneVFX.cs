using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneVFX : MonoBehaviour
{
    public enum Mode
    {
        CANDLE,
        SCRY
    }
    public float candleTargetIntensity;
    public float candleFlickerNoise;
    public float scryTargetIntensity;
    public float scryParticlesTargetRate;
    public float scryParticlesTargetLifeTime;
    public float scryBackgroundTargetOpacity;
    public float transitionDuration;

    public GameObject CandleLight;
    public GameObject ScryLight;
    public GameObject ScryParticle;
    public GameObject ScryBackground;

    private Mode currentMode = Mode.CANDLE;
    private Mode targetMode =  Mode.CANDLE;
    private float transitionStartTime;
    private bool isTransitioning = false;
   
    public void SwitchMode(Mode targetMode)
    {
        if(targetMode != currentMode)
        {
            transitionStartTime = Time.time;
            this.targetMode = targetMode;
            isTransitioning = true;

            if(currentMode == Mode.SCRY)
            {
                ParticleSystem.EmissionModule scryModeParticleEmission = ScryParticle.GetComponent<ParticleSystem>().emission;
                scryModeParticleEmission.rateOverTime = 0;
                
            }

            if(targetMode == Mode.SCRY)
            {
                ParticleSystem.EmissionModule scryModeParticleEmission = ScryParticle.GetComponent<ParticleSystem>().emission;
                scryModeParticleEmission.rateOverTime = scryParticlesTargetRate;
            }
        }
    }
    
    void Update()
    {
        if(isTransitioning)
        {
            float advancement = (Time.time - transitionStartTime) / transitionDuration;
            if(advancement >= 1)
            {
                advancement = 1;
                isTransitioning = false;
            }

            switch (currentMode)
            {
                case Mode.CANDLE:
                    CandleLight.GetComponent<Light>().intensity = candleTargetIntensity * (1 - advancement);
                    break;

                case Mode.SCRY:
                    ScryLight.GetComponent<Light>().intensity = scryTargetIntensity * (1 - advancement);
                    Color currentBGColor = ScryBackground.GetComponent<RawImage>().color;
                    currentBGColor.a = scryBackgroundTargetOpacity * (1 - advancement);
                    ScryBackground.GetComponent<RawImage>().color = currentBGColor;
                    break;
            }

            switch (targetMode)
            {
                case Mode.CANDLE:
                    CandleLight.GetComponent<Light>().intensity = candleTargetIntensity * advancement;
                    break;

                case Mode.SCRY:
                    ScryLight.GetComponent<Light>().intensity = scryTargetIntensity * advancement;
                    Color currentBGColor = ScryBackground.GetComponent<RawImage>().color;
                    currentBGColor.a = scryBackgroundTargetOpacity * advancement;
                    ScryBackground.GetComponent<RawImage>().color = currentBGColor;
                    break;
            }

            if(!isTransitioning)
            {
                currentMode = targetMode;
            }

        }
        else
        {
            if(currentMode == Mode.CANDLE)
            {
                //CandleLight.GetComponent<Light>().intensity = candleTargetIntensity + Mathf.Sin(Time.time*10) * candleFlickerNoise; on verra plus tard
            }
        }


    }
}
