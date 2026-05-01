using UnityEngine;
using Elyqara.Characters;

namespace Elyqara.Player
{
    public sealed class PlayerCharacterBinder : MonoBehaviour
    {
        [SerializeField] private CharacterData character;
        public CharacterData Character => character;
    }
}
