using UnityEngine;
using UnityEngine.SceneManagement;

public class UIControl : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Button scene changers
    public void startGame() //for art prototype
    {
        SceneManager.LoadScene("Nursery_Blockout");
    }

    public void homeScreen()
    {
        SceneManager.LoadScene("Home");
    }

    public void zooScreen()
    {
        SceneManager.LoadScene("Zoo");
    }

    public void excavateSceneSelect()
    {
        SceneManager.LoadScene("Minigame_Blockout");
    }

    public void penScreen()
    {
        SceneManager.LoadScene("Nursery_Blockout");
    }

    public void herbSelect()
    {
        SceneManager.LoadScene("Herbivore_Select");
    }

}
