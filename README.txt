Othello

Original code courtesy of Jared Okun and Sam Gould

This project essentially requires you to build the search tree algorithm and a good static evaluation function (SEF) to go with. The first of these, while perhaps tricky initially, should be reasonably stable once you get it working. The latter is where you get to create the most powerful player that you can.

For this assignment, please build two AI's using related but different algorithms.


You should attempt to build your AI with at least AB Negamax or Negascout. You should make available a public variable that stores MaxDepth, which of course your search depth must stop at.

For the SEF, there are many approaches you can take, and you can use some of these in combination:

	compute the greatest number of pieces of your color on the board
	maybe vary this by seeking a lower count when in mid game
	corner squares are ultra-desirable
	each of the squares could be given its own desirability score
	you could introduce some randomness by choosing between identically scored best moves
	do some research and see what else you can come up with

There are only three modules supplied in the Othello Framework. The one called BoardScript.cs does nearly all the heavy lifting for everthing except the AI. It works fine and I recommend not tampering with it. Note that it conveniently displays little dots in every square where the human player can legally play.

Note especially the method GetValidMoves(). This method returns a List of KeyValuePair. TheKeyValuePair is a useful class in C#/.NET that contains pretty much what it sounds like. You will need to call GetValidMoves all over your AI code. Note that it is passed in the turnNumber (which should be plyNumber) and the code uses that modulo 2 to determine which player it is obtaining legal moves for.

The second module in the code is AIScript.cs. This is a "thin" parent class that your AI will descend from. All it offers is the method MakeMove








There is also a NegamaxEval function that uses a less optimized version of negamax and a completely random AI.

These can be toggled as well as if it is human or AI on the board.

