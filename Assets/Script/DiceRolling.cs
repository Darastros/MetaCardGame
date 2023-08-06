using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRolling : MonoBehaviour
{
    public GameObject explosionVFX;
    public Vector3 launchTarget = new Vector3(0, 0, -2.8f);
    public float launchDuration = 1.0f;
    private bool isLaunching = false;
    private float launchStartTime = 0;
    private Vector3 launchStartPosition = new Vector3();
    private Vector3 launchSpeed = new Vector3();
    private Vector3 launchGravity = new Vector3();
    private float randomLaunchHeight = 0;
    private int tryGetResultAmount = 0;
    private int rerollAmount = 0;
    private int diceResult = 0;
    void FixedUpdate()
    {
        GetComponent<Rigidbody>().AddForce(Physics.gravity * GetComponent<Rigidbody>().mass * 3.0f);
        if(isLaunching)
        {
            float currentLaunchDuration = Time.time - launchStartTime;
            if (currentLaunchDuration <= 2 * launchSpeed.y / -launchGravity.y)
                GetComponent<Rigidbody>().position = launchStartPosition + launchSpeed * currentLaunchDuration + 0.5f * launchGravity * currentLaunchDuration * currentLaunchDuration;
            else
            {
                GameManager.Instance.OnDiceResult(gameObject, diceResult);
                Instantiate(explosionVFX, GetComponent<Rigidbody>().position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
            
    }

    public void RollTheDice()
    {
        Vector2 randomTarget = new Vector2(Random.Range(-5.0f, 5.0f), -3.38f);
        Vector3 randomLaunchAxis = Quaternion.Euler(0, Random.Range(0,360), 0) * new Vector3(1, 0, 0);
        Vector2 tMin = (new Vector2(-10.3f, -9.2f) - randomTarget) / new Vector2(randomLaunchAxis.x, randomLaunchAxis.z);
        Vector2 tMax = (new Vector2(10.3f, 2.6f) - randomTarget) / new Vector2(randomLaunchAxis.x, randomLaunchAxis.z);
        Vector2 t1 = Vector2.Min(tMin, tMax);
        Vector2 t2 = Vector2.Min(tMin, tMax);
        float tNear = Mathf.Max(t1.x, t1.y);
        float tFar = Mathf.Min(t2.x, t2.y);
        transform.position = new Vector3(randomTarget.x, 2.0f, randomTarget.y) + randomLaunchAxis * tNear;
        tryGetResultAmount = 0;
        transform.rotation = Random.rotation;
        GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(-1.0f,1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * 1000.0f, ForceMode.Impulse);
        GetComponent<Rigidbody>().AddForce(randomLaunchAxis * 20.0f, ForceMode.VelocityChange);
        Invoke("ReturnDiceResult", 0.75f);
    }

    public void ReturnDiceResult()
    {
        if(Vector3.Dot(transform.forward, Vector3.up) > 0.8f)
            diceResult = 1;
        else if (Vector3.Dot(-transform.forward, Vector3.up) > 0.8f)
            diceResult = 6;
        else if (Vector3.Dot(transform.up, Vector3.up) > 0.8f)
            diceResult = 2;
        else if (Vector3.Dot(-transform.up, Vector3.up) > 0.8f)
            diceResult = 5;
        else if (Vector3.Dot(transform.right, Vector3.up) > 0.8f)
            diceResult = 4;
        else if (Vector3.Dot(-transform.right, Vector3.up) > 0.8f)
            diceResult = 3;

        if(diceResult == 0)
        {
            if(tryGetResultAmount < 1)
            {
                Invoke("ReturnDiceResult", 0.25f);
                tryGetResultAmount++;
                return;
            }
            else if(rerollAmount < 2)
            {
                rerollAmount++;
                RollTheDice();
                return;
            }
        }
        LaunchDice();
    }

    private void LaunchDice()
    {
        isLaunching = true;
        launchStartTime = Time.time;
        randomLaunchHeight = Random.Range(3.0f, 5.0f);
        launchGravity = new Vector3(0, -1, 0) * 8 * randomLaunchHeight / (launchDuration * launchDuration);
        launchStartPosition = GetComponent<Rigidbody>().position;
        Vector3 horizontalSpeedVector = (launchTarget - launchStartPosition) / launchDuration;
        launchSpeed = new Vector3(horizontalSpeedVector.x, Mathf.Sqrt(-launchGravity.y * randomLaunchHeight * 2), horizontalSpeedVector.z);
    }
}
