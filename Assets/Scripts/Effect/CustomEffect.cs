using System.Collections;
using UnityEngine;

public class CustomEffect : MonoBehaviour {
    public CustomEffect GetEffect() {
        return EffectPool.GetInstance().GetEffect(this);
    }
    public CustomEffect GetEffect(Vector3 position) {
        var effect = EffectPool.GetInstance().GetEffect(this);
        effect.transform.position = position;
        return effect;
    }
    public ParticleSystem Particle { get; private set; }
    public AudioSource Audio { get; private set; }
    public LineRenderer Line { get; private set; }
    public TrailRenderer Trail { get; private set; }

    private void Awake() {
        Particle = GetComponent<ParticleSystem>();
        Audio = GetComponent<AudioSource>();
        Line = GetComponent<LineRenderer>();
        Trail = GetComponent<TrailRenderer>();
    }

    private void OnEnable() {
        if (Particle) {
            Particle.Play();
        }
        if (Audio) {
            Audio.Play();
        }
        if (Trail) {
            Trail.Clear();
        }
    }

    private void OnDisable() {
        if (Particle) {
            Particle.Stop();
        }
        if (Audio) {
            Audio.Stop();
        }
        if (Trail) {
            Trail.Clear();
        }
    }

    public void SetLine(Vector3 start, Vector3 end, float duration) {
        if (Line) {
            Line.SetPositions(new Vector3[] { start, end });
            StartCoroutine(DisableCoroutine(duration));
        }
    }

    private IEnumerator DisableCoroutine(float duration) {
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
    }

    private void Update() {
        if (Particle) {
            if (!Particle.IsAlive()) {
                gameObject.SetActive(false);
            }
        }
    }
}
