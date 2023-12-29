using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD {
    public class UICockpit : MonoBehaviour {
        
        public HUDRadar radar = default;
        public HUDPitchAndRoll pitchAndRoll = default;
        public HUDStatus status = default;
        
        public Text altitude = default;
        public Text speed = default;
        public Text landingGear = default;

        private Player _player = null;
        public void Init(Player player) {
            _player = player;
            if (!_player) {
                gameObject.SetActive(false);
                return;
            }

            var aeroplane = _player.aeroplane;
            pitchAndRoll.Init(aeroplane);
            radar.Init(player);
            status.Init(player);
        }

        void LateUpdate() {
            if (!_player) {
                gameObject.SetActive(false);
                return;
            }
            altitude.text = ((int)_player.aeroplane.altitude).ToString();
            speed.text = ((int)_player.aeroplane.rigidbody.velocity.magnitude).ToString();
            landingGear.text = _player.aeroplane.landingGear ? "OPENED" : "CLOSED";
        }
        
    }
}