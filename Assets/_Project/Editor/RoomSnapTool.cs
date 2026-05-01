using Elyqara.Dungeon;
using UnityEditor;
using UnityEngine;

namespace Elyqara.EditorTools
{
    // 방 끼리 붙이는 도구. 단계 6 핵심.
    // 사용법:
    //   1. Hierarchy 에서 anchor Door socket (안 움직일 방의 Door) 클릭
    //   2. Cmd 잡고 source Door socket (이동할 방의 Door) 추가 클릭
    //   3. Tools/Elyqara/Snap Selected Doors 메뉴 (또는 Cmd+Alt+S)
    // 결과: source Room 이 이동해서 source Door world position == anchor Door world position.
    //       두 방의 mesh 가 정확히 맞붙음.
    public static class RoomSnapTool
    {
        [MenuItem("Tools/Elyqara/Snap Selected Doors %&s")]
        private static void SnapSelectedDoors()
        {
            var selected = Selection.gameObjects;
            if (selected == null || selected.Length != 2)
            {
                Debug.LogError("[RoomSnap] Door socket 2개 선택 필요. 먼저 anchor Door 클릭, Cmd 잡고 source Door 추가 클릭.");
                return;
            }

            var active = Selection.activeGameObject;
            GameObject anchor;
            GameObject source;

            // active = 마지막에 클릭한 것 = source (이동할 쪽)
            if (selected[0] == active)
            {
                source = selected[0];
                anchor = selected[1];
            }
            else
            {
                source = selected[1];
                anchor = selected[0];
            }

            var anchorRoom = anchor.GetComponentInParent<Room>();
            var sourceRoom = source.GetComponentInParent<Room>();

            if (anchorRoom == null || sourceRoom == null)
            {
                Debug.LogError("[RoomSnap] 선택한 두 GameObject 모두 Room 컴포넌트가 있는 방의 자식이어야 함.");
                return;
            }
            if (anchorRoom == sourceRoom)
            {
                Debug.LogError("[RoomSnap] 같은 방의 Door 두 개 선택. 다른 방의 Door 끼리 선택해야 함.");
                return;
            }

            Undo.RecordObject(sourceRoom.transform, "Snap Room To Door");
            var delta = anchor.transform.position - source.transform.position;
            sourceRoom.transform.position += delta;

            Debug.Log($"[RoomSnap] '{sourceRoom.name}' 을 '{anchorRoom.name}' 의 '{anchor.name}' 위치에 정렬 (delta={delta})");
            EditorUtility.SetDirty(sourceRoom.transform);
        }

        [MenuItem("Tools/Elyqara/Snap Selected Doors %&s", true)]
        private static bool SnapSelectedDoorsValidate()
        {
            return Selection.gameObjects != null && Selection.gameObjects.Length == 2;
        }
    }
}
