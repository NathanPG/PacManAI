using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class AIScript {

    protected BoardSpace color;

    /// <summary>
    /// This is the way to make a move happen. Note the definition for how the board is 
    /// represented (a 2D array of Enums) and how the return value is handled (the score 
    /// and the move as a KeyValuePair). Your code will need to supply the actual 
    /// code in the descendent class that you write. 
    /// </summary>
    /// <param name="availableMoves"></param>
    /// <param name="currentBoard"></param>
    /// <returns></returns>
    public abstract KeyValuePair<int, int> makeMove(List<KeyValuePair<int, int>> availableMoves, BoardSpace[][] currentBoard);

    /// <summary>
    /// Displays the piece on the board.
    /// </summary>
    /// <param name="color"></param>
    public void setColor(BoardSpace color) {
        this.color = color;
    }

}
