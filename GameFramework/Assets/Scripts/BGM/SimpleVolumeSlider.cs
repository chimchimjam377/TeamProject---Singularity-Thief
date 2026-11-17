using UnityEngine;
using UnityEngine.UI;

public class SimpleVolumeSlider : MonoBehaviour
{
    public AudioSource bgmSource;
    public Slider bgmSlider;

    private void Start()
    {
        // 슬라이더 초기값을 현재 오디오 볼륨과 맞춤
        bgmSlider.value = bgmSource.volume;

        // 슬라이더를 움직일 때마다 OnSliderChanged 함수가 실행되도록 연결
        bgmSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        bgmSource.volume = value;
    }
}