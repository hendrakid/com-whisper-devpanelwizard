using com.whisper.devpanelwizard;
using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    [ExposeVariable]
    public bool isDead;

    [ActionButton]
    public void GetHit(int damage, bool fromBehind)
    {
        var direction = fromBehind ? "behind" : "front";
        Debug.Log($"attack comming from {direction},get damage {damage}");
    }

    [ActionButton]
    public void FindPlayer(string playerName)
    {
        Debug.Log($"looking for {playerName}!");
    }

    [ActionButton]
    private void Dead()
    {
        isDead = true;
        Debug.Log($"{name} die!");
    }

    [ActionButton]
    private void Respawn()
    {
        isDead = false;
        Debug.Log($"{name} respawn!");
    }
}
