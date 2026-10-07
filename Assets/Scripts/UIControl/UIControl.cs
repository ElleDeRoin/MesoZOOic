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
        SceneManager.LoadScene("Zoo");
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

    public void carnoSelect()
    {
        SceneManager.LoadScene("Carnivore_Select");
    }

    public void carnoPen()
    {
        SceneManager.LoadScene("Carno_Blockout");
    }
    
    public void gachaScreen()
    {
        SceneManager.LoadScene("Gacha_Screen");
    }

    public void gachaSelect()
    {
        SceneManager.LoadScene("Minigame_Select");
    }

    public void paidGacha()
    {
        SceneManager.LoadScene("Minigame_Paid");
    }

}
