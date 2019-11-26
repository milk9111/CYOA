using UnityEngine.SceneManagement;

namespace Commands
{
    public class Exit : Command
    {
        public override void Execute(string[] args)
        {
            SceneManager.LoadScene("start_screen");
        }
    }
}