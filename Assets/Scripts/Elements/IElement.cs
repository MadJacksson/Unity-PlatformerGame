using UnityEngine;

public interface IElement
{
    void OnHoldStart(Vector2 position);
    void OnHoldUpdate(Vector2 position);
    void OnHoldEnd(Vector2 position);
}
