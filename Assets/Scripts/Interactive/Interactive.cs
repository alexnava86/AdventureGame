using UnityEngine;

public abstract class Interactive : MonoBehaviour
{
	protected delegate void Interact(AbstractCharacter character);
	protected Interact interact;
}