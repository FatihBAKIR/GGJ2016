using UnityEngine;

public class SpriteBlink : MonoBehaviour
{
    private float _change = 1;
    private float _t = 0;

    void Update()
    {
        GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), _t);
        
        _t += _change * Time.deltaTime / 2;

        if (_t > 1.0f || _t < 0)
        {
            _change *= -1;
        }
    }
}
