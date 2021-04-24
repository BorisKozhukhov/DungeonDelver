using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMove : MonoBehaviour
{
    private IFacingMover mover;

    private void Awake()
    {
        mover = GetComponent<IFacingMover>();
    }

    private void FixedUpdate()
    {
        // Если объект не перемещается - выйти
        if (!mover.Moving)
            return;
        int facing = mover.GetFacing();

        // Если объект перемещается, применить выравнивание по сетке
        // Сначала получить координаты ближайшего узла сетки
        Vector2 rPos = mover.RoomPos;
        Vector2 rPosGrid = mover.GetRoomPosOnGrid(); // Этот код полагается на интерфейс IFacingMover для определения шага сетки

        // Подвинуь объект в сторону линии сетки
        float delta = 0;
        if (facing == 0 || facing == 2) // Движение по горизонтали, выравнивание по оси у
            delta = rPosGrid.y - rPos.y;
        else // Движение по вертикали, выравнивание по оси х
            delta = rPosGrid.x - rPos.x;
        if (delta == 0)
            return; // Объект уже выровнен по сетке
        float move = mover.GetSpeed() * Time.fixedDeltaTime;
        move = Mathf.Min(move, Mathf.Abs(delta));
        if (delta < 0)
            move = -move;
        if (facing == 0 || facing == 2)
            rPos.y += move;
        else
            rPos.x += move;
        mover.RoomPos = rPos;
    }
}
