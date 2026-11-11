using UnityEngine;

public class StoryTrigger2D : MonoBehaviour
{
    [SerializeField] private StoryManager storyManager;
    [SerializeField] private StoryData storyData;

    [SerializeField] private bool playOnce = true;

    private bool hasPlayed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasPlayed && playOnce)
            return;

        if (!other.CompareTag("Player"))
            return;

        hasPlayed = true;

        storyManager.PlayStory(storyData);
    }
}