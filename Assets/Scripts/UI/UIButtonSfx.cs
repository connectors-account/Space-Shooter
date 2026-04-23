using SpaceShooter.Audio;
using UnityEngine;

namespace SpaceShooter.UI
{
    public class UIButtonSfx : MonoBehaviour
    {
        public void PlayClick()
        {
            SoundManager.Instance?.PlayUIClick();
        }
    }
}
