using UnityEngine;
using UnityEngine.UI;

public class UIScoreController : MonoBehaviour
{
    [Header("UI")]
    public Text score;
    public Text highScore;

    [Header("Score")]
    public ScoreController scoreController;

    private void Update()
    {
        // score
        score.text = scoreController.GetCurrentScore().ToString();
        // high score
        highScore.text = ScoreData.highScore.ToString();
    }
}
