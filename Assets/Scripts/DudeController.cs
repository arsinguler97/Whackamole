using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class DudeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light2D[] holeLights;   // 5 Light2D components (URP 2D lights)
    [SerializeField] private MoleController mole;    // Reference to mole script
    [SerializeField] private float attackInterval = 5f;
    [SerializeField] private float dimDuration = 1f; // How long to dim before hitting

    private int _currentTarget = -1;

    void Start()
    {
        StartCoroutine(AttackLoop());
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);
            ChooseRandomHole();
            yield return StartCoroutine(DimAndHit());
        }
    }

    private void ChooseRandomHole()
    {
        int newTarget;
        do
        {
            newTarget = Random.Range(0, holeLights.Length);
        } while (newTarget == _currentTarget);

        _currentTarget = newTarget;
    }

    private IEnumerator DimAndHit()
    {
        Light2D target = holeLights[_currentTarget];
        float startIntensity = target.intensity;

        // Gradually dim the light
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / dimDuration;
            target.intensity = Mathf.Lerp(startIntensity, 0f, t);
            yield return null;
        }

        // Hit moment (light completely off)
        CameraShake.Instance.Shake(0.2f, 0.2f);

        // Check if mole is up at the same hole
        if (mole.IsUp() && mole.GetCurrentHoleIndex() == _currentTarget)
        {
            mole.Die();
        }

        // Wait and restore light
        yield return new WaitForSeconds(0.5f);
        target.intensity = startIntensity;
    }
}