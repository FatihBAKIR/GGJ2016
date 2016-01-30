using System;
using UnityEngine;
using System.Collections;

public class TileWorks : MonoBehaviour
{
    public event Action<Tile> StateChangeComplete = delegate { };

    enum ChangeState
    {
        None,
        Ascending,
        Shading,
        Lighting,
        FadingIn,
        FadingOut
    }

    private Tile _tile;
    private Vector3 _from;
    private Vector3 _to;
    private float _t = -1;

    private SeeState _prev;

    private Color _colorFrom;
    private Color _colorTo;

    private ChangeState _state;

    private bool _ascended;

    void Awake()
    {
        _prev = SeeState.Hidden;
        _state = ChangeState.None;
        _ascended = false;
    }

    public bool SetState(Tile t, SeeState st)
    {
        _tile = t;
        bool ret = false;

        if (!_ascended && st > SeeState.Explored && _prev <= SeeState.Hidden)
        {
            Ascend();
            ret = true;
        }

        if (st == SeeState.Explored && _prev > SeeState.Explored)
        {
            Shade();
        }

        if (_prev == SeeState.Explored && st > SeeState.Explored)
        {
            Light();
        }

        _prev = st;

        return ret;
    }

    void Ascend()
    {
        _state = ChangeState.Ascending;
        _t = 0;
        _to = Level.CurrentLevel.Grid.CoordToPosition(_tile.Coordinate) + (_tile.Elevation * Vector3.up);
        _from = transform.position;

        _colorFrom = GetComponent<Renderer>().material.color;
        _colorTo = Color.white;
    }

    void Shade()
    {
        _state = ChangeState.Shading;

        _from = transform.position;
        _to = transform.position;

        _colorFrom = GetComponent<Renderer>().material.color;
        _colorTo = Color.gray;
        _t = 0;
    }

    void Light()
    {
        _state = ChangeState.Lighting;

        _from = transform.position;
        _to = transform.position;

        _colorFrom = GetComponent<Renderer>().material.color;
        _colorTo = Color.white;
        _t = 0;
    }

    private bool _isFadedOut;
    private int _prevSrcBlend, _prevDstBlend, _prevZWrite, _prevRenderQueue;
    public void FadeOut()
    {
        Material mat = GetComponent<Renderer>().material;

        _prevSrcBlend = mat.GetInt("_SrcBlend");
        _prevDstBlend = mat.GetInt("_DstBlend");
        _prevZWrite = mat.GetInt("_ZWrite");
        _prevRenderQueue = mat.renderQueue;

        mat.SetFloat("_Mode", 2);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        _state = ChangeState.FadingOut;

        _isFadedOut = true;
        _colorFrom = GetComponent<Renderer>().material.color;
        _colorTo = new Color(_colorFrom.r, _colorFrom.g, _colorFrom.b, 0.5f);
        _t = 0;

        GetComponent<Collider>().enabled = false;
    }

    public void FadeIn()
    {
        if (!_isFadedOut)
        {
            return;
        }

        _state = ChangeState.FadingIn;
        GetComponent<Collider>().enabled = true;

        _colorFrom = GetComponent<Renderer>().material.color;
        _colorTo = new Color(_colorFrom.r, _colorFrom.g, _colorFrom.b, 1f);
        _t = 0;
    }

    public void Update()
    {
        if (_t < 0) return;

        switch (_state)
        {
            case ChangeState.None:
                break;
            case ChangeState.Ascending:
                transform.position = Vector3.Lerp(_from, _to, _t);
                GetComponent<Renderer>().material.color = Color.Lerp(_colorFrom, _colorTo, _t);
                break;
            case ChangeState.Shading:
            case ChangeState.Lighting:
            case ChangeState.FadingIn:
            case ChangeState.FadingOut:
                GetComponent<Renderer>().material.color = Color.Lerp(_colorFrom, _colorTo, _t);
                GetComponent<Renderer>().material.color = Color.Lerp(_colorFrom, _colorTo, _t);
                break;
        }

        _t += Time.deltaTime * 1.5f;

        if (_t < 1) return;

        transform.position = _to;
        _t = -1;

        switch (_state)
        {
            case ChangeState.Ascending:
                _ascended = true;
                Level.CurrentLevel.Reveal(_tile.Coordinate);
                StateChangeComplete(_tile);
                break;

            case ChangeState.FadingIn:
                Material mat = GetComponent<Renderer>().material;

                mat.SetFloat("_Mode", 0);
                mat.SetInt("_SrcBlend", _prevSrcBlend);
                mat.SetInt("_DstBlend", _prevDstBlend);
                mat.SetInt("_ZWrite", _prevZWrite);
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = _prevRenderQueue;

                break;
        }

        _state = ChangeState.None;
    }
}
