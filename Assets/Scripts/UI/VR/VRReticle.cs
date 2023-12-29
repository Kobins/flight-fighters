using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.VR {
    /// <summary>
    /// UI 상호작용하기 위한 Reticle 구현
    /// </summary>
    public class VRReticle : MonoBehaviour {
        [SerializeField] private RectTransform _reticle;
        [SerializeField] private float _reticleSelectScale = 1.2f;
        [SerializeField] private float _reticleScaleChangeSpeed = 10f;
        [SerializeField] private Image _progressImage;
        [SerializeField] private float _progressAlphaChangeSpeed = 10f;
        [SerializeField] private Text _recenterText;

        private bool _isSelecting = false;
        private Vector2 _reticleInitialScaleVector;
        private Vector2 _reticleSelectScaleVector;
        private CanvasGroup _progressCanvasGroup;
        private void Start() {
            _reticleInitialScaleVector = _reticle.localScale;
            _reticleSelectScaleVector = new Vector2(_reticleSelectScale, _reticleSelectScale);
            _progressCanvasGroup = _progressImage.transform.GetComponent<CanvasGroup>();
            ResetProgress();
        }

        private void Update() {
            // 비활성화 됨
            if (!_isSelecting || (_progressImage.fillAmount >= 1f || _progressImage.fillAmount <= 0f)) {
                _reticle.localScale = Vector2.Lerp(
                    _reticle.localScale, _reticleInitialScaleVector,
                    Time.deltaTime * _reticleScaleChangeSpeed
                );
                _progressCanvasGroup.alpha = Mathf.Lerp(
                    _progressCanvasGroup.alpha, 0f, 
                    Time.deltaTime * _progressAlphaChangeSpeed
                );
            }
            // 활성화됨
            else {
                _reticle.localScale = Vector2.Lerp(
                    _reticle.localScale, _reticleSelectScaleVector,
                    Time.deltaTime * _reticleScaleChangeSpeed
                );
                _progressCanvasGroup.alpha = Mathf.Lerp(
                    _progressCanvasGroup.alpha, 1f, 
                    Time.deltaTime * _progressAlphaChangeSpeed
                );
            }
        }
        
        public void UpdateProgress(float normalized) {
            _isSelecting = normalized < 1f;
            // 반대로: progress는 어두운 쪽의 이미지임
            _progressImage.fillAmount = 1f - normalized;
        }

        public void UpdateProgressByRecenter(float normalized) {
            UpdateProgress(normalized);
            _recenterText.gameObject.SetActive(true);
        }

        public void ResetProgress() {
            _isSelecting = false;
            _recenterText.gameObject.SetActive(false);
        }
    }
}