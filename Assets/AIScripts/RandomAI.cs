using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAI : AIScript {

    /// <summary>
    /// This shows how to override the abstract definition of makeMove. All this one
    /// does is stupidly a random, yet legal, move.
    /// </summary>
    /// <param name="availableMoves"></param>
    /// <param name="currentBoard"></param>
    /// <returns></returns>

    public override KeyValuePair<int, int> makeMove(List<KeyValuePair<int, int>> availableMoves, BoardSpace[][] currentBoard) {

        return availableMoves[Random.Range(0, availableMoves.Count)];
    }

}
