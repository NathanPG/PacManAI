using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public enum BoardSpace {
    EMPTY,
    BLACK,
    WHITE,
}

/// <summary>
/// BoardScript does nearly all the heavy lifting for everthing except the AI. It 
/// works fine and I recommend not tampering with it. Note that it conveniently 
/// displays little yellow dots in every square where the human player can legally 
/// play.
/// </summary>
public class BoardScript : MonoBehaviour {

    public GameObject piecePrefab;
    BoardSpace[][] board;
    GameObject[][] boardGameObjects;

    uint turnNumber;    // Technically, this would be better named plyNumber, two plies per turn
    bool gameStarted;
    bool gameEnded;

    List<KeyValuePair<int, int>> currentValidMoves;

    public bool isPlayerOneAI;
    public bool isPlayerTwoAI;

    public string playerOneScriptClassName;
    public string playerTwoScriptClassName;

    
    AIScript playerOneScript;
    AIScript playerTwoScript;
    public Text bText;
    public Text wText;
    public Text tText;
    List<GameObject> possibleMovesArray;
    bool posMovesShown;
    void Awake() {
        //determines which side is player

        /*
         * Set these next two booleans to control who is playing which side. All four combinations
         * are possible: Human-Human, Human-AI, AI-Human, AI-AI. In all cases, black plays first.
         * 
         * IMPORTANT: even though both of these variables are "public" and are exposed in the 
         * Inspector, changing them has no effect for some reason. This is a known issue.
         * 
         * To get the paring you want, force those values here instead. 
         * 
         * If you are feeling ambitious, you could add a button to the UI that allows the player
         * to change who-plays-what and which AI to use to avoid having to change these manually.
         */

        isPlayerOneAI = false;
        isPlayerTwoAI = true;

        /* For the calls to System.Reflection.Assembly.GetExecutingAssembly() below, enter the
         * string that names your .cs module that contains your AI code, such as is shown here
         * for RandomAI. Do this in two places if you want to let the AI play either as Black
         * or White. Or you could try dragging the module name into the "Player One Script Class" 
         * or ""Player Two Script Class" in the Inspector for the Reversi game board.
         */

        possibleMovesArray = new List<GameObject>();
        if (isPlayerOneAI) {
            System.Type scriptType = System.Reflection.Assembly.GetExecutingAssembly().GetType("RandomAI");
            //System.Type scriptType = System.Reflection.Assembly.GetExecutingAssembly().GetType(playerOneScriptClassName);
            System.Object o = Activator.CreateInstance(scriptType);
            playerOneScript = (AIScript)o;
            playerOneScript.setColor(BoardSpace.BLACK);
        }
        if (isPlayerTwoAI) {
            //System.Type scriptType = System.Reflection.Assembly.GetExecutingAssembly().GetType(playerTwoScriptClassName);
            System.Type scriptType = System.Reflection.Assembly.GetExecutingAssembly().GetType("RandomAI");
            System.Object o = Activator.CreateInstance(scriptType);
            playerTwoScript = (AIScript)o;
            playerTwoScript.setColor(BoardSpace.WHITE);
        }

        InitBoard();

    }



    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (!gameEnded) {
            if(turnNumber %2 == 0)
            {
                tText.text = "Current Turn: Black";
            }
            else
            {
                tText.text = "Current Turn: White";
            }
            if (turnNumber % 2 == 0 && isPlayerOneAI) {

                KeyValuePair<int, int> move = playerOneScript.makeMove(currentValidMoves, board);
                PlacePiece(move.Value, move.Key);
            } else if (turnNumber % 2 == 1 && isPlayerTwoAI) {

                KeyValuePair<int, int> move = playerTwoScript.makeMove(currentValidMoves, board);
                PlacePiece(move.Value, move.Key);

            } else {
                if (!posMovesShown)//shows potential moves for player character
                {
                    foreach (KeyValuePair<int, int> a in currentValidMoves)
                    {
                        GameObject piece = Instantiate(piecePrefab, transform);
                        SpriteRenderer spriteR = piece.GetComponent<SpriteRenderer>();
                        piece.transform.localPosition = new Vector3((float)a.Value - 3.5f, (float)a.Key - 3.5f, 0f);
                        piece.GetComponent<SpriteRenderer>().color = Color.yellow;
                        piece.transform.localScale = new Vector3(.4f, .4f);
                        possibleMovesArray.Add(piece);
                        posMovesShown = true;
                    }
                }
                if (Input.GetMouseButtonUp(0)) {
                    Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                    if (clickPosition.y < 4.0 && clickPosition.y > -4.0 && clickPosition.x > -4.0 && clickPosition.x < 4.0) {
                        int clickX = Mathf.FloorToInt(clickPosition.x) + 4;
                        int clickY = Mathf.FloorToInt(clickPosition.y) + 4;
                        if (currentValidMoves.Contains(new KeyValuePair<int, int>(clickY, clickX))) {
                            PlacePiece(clickX, clickY);
                            foreach(GameObject a in possibleMovesArray)
                            {
                                Destroy(a);
                                posMovesShown = false;
                            }
                        }
                    }

                }
            }
        }
    }

    private void InitBoard() { //set up board
        board = new BoardSpace[8][];
        boardGameObjects = new GameObject[8][];
        turnNumber = 0;
        gameStarted = false;
        for (int i = 0; i < 8; ++i) {
            board[i] = new BoardSpace[8];
            boardGameObjects[i] = new GameObject[8];
            for (int j = 0; j < 8; ++j) {
                board[i][j] = BoardSpace.EMPTY;
                boardGameObjects[i][j] = null;
            }
        }
        PlacePiece(3, 3);
        PlacePiece(3, 4);
        PlacePiece(4, 4);
        PlacePiece(4, 3);
        gameStarted = true;
        List<KeyValuePair<int, int>> moves = GetValidMoves(board, turnNumber);

        currentValidMoves = moves;
    }

    public void restartGame() {
        SceneManager.LoadScene(0);
    }

    public void PlacePiece(int x, int y) { //instantiate piece at position and add to that side's points
        GameObject piece = Instantiate(piecePrefab, transform);
        SpriteRenderer spriteR = piece.GetComponent<SpriteRenderer>();
        piece.transform.localPosition = new Vector3((float)x - 3.5f, (float)y - 3.5f, 0f);
        if (turnNumber % 2 == 0) {
            spriteR.color = Color.black;
            board[y][x] = BoardSpace.BLACK;
        } else {
            board[y][x] = BoardSpace.WHITE;
        }
        boardGameObjects[y][x] = piece;
        if (gameStarted) {
            List<KeyValuePair<int, int>> changedSpaces = GetPointsChangedFromMove(board, turnNumber, x, y);
            foreach(KeyValuePair<int, int> space in changedSpaces) {
                SpriteRenderer spriteRenderer = boardGameObjects[space.Key][space.Value].GetComponent<SpriteRenderer>();  
                if (turnNumber % 2 == 0) {
                    spriteRenderer.color = Color.black;
                    board[space.Key][space.Value] = BoardSpace.BLACK;
                } else {
                    spriteRenderer.color = Color.white;
                    board[space.Key][space.Value] = BoardSpace.WHITE;
                }
            }
            currentValidMoves = GetValidMoves(board, turnNumber + 1);
            if(currentValidMoves.Count == 0) {
                ++turnNumber;
                currentValidMoves = GetValidMoves(board, turnNumber + 1);
                if(currentValidMoves.Count == 0) {
                    GameOver();
                }
            }
        }
        int blackCount = 0;
        int whiteCount = 0;
        foreach (BoardSpace[] row in board)
        {
            foreach (BoardSpace space in row)
            {
                switch (space)
                {
                    case (BoardSpace.BLACK):
                        blackCount++;
                        break;
                    case (BoardSpace.WHITE):
                        whiteCount++;
                        break;
                }
            }
        }
        bText.text = "Black Score: " + blackCount;
        wText.text = "White Score: " + whiteCount;
        ++turnNumber;
    }

    public static List<KeyValuePair<int, int>> GetPointsChangedFromMove(BoardSpace[][] board, uint turnNumber, int x, int y) {
        //determines how much a move changed the overall point value
        BoardSpace enemyColor = turnNumber % 2 == 0 ? BoardSpace.WHITE : BoardSpace.BLACK;
        BoardSpace ourColor = turnNumber % 2 == 0 ? BoardSpace.BLACK : BoardSpace.WHITE;
        if (board.Length != 8 || board[0].Length != 8 || y < 0 || y >= 8 || x < 0 || x >= 8 || board[y][x] != ourColor) {
            return null;
        }

        List<KeyValuePair<int, int>> changedSpaces = new List<KeyValuePair<int, int>>();

        for (int k = -1; k < 2; ++k) {
            for (int l = -1; l < 2; ++l) {
                if (!((k == 0 && l == 0) || k + y < 0 || k + y >= 8 || l + x < 0 || l + x >= 8) && board[k + y][l + x] == enemyColor) {
                    int multiplier = 2;
                    while (k * multiplier + y >= 0 && k * multiplier + y < 8 && l * multiplier + x >= 0 && l * multiplier + x < 8) {
                        if (board[k * multiplier + y][l * multiplier + x] == BoardSpace.EMPTY) {
                            break;
                        } else if (board[k * multiplier + y][l * multiplier + x] == ourColor) {
                            for(int i = multiplier - 1; i >= 1; --i) {
                                changedSpaces.Add(new KeyValuePair<int, int>(k * i + y, l * i + x));
                            }
                            break;
                        }
                        ++multiplier;
                    }

                }
            }
        }

        return changedSpaces;
    }

    public static List<KeyValuePair<int, int>> GetValidMoves(BoardSpace[][] board, uint turnNumber) {
        if(board.Length != 8 || board[0].Length != 8) {
            return null;
        }
        //determines the places that either player can move
        List<KeyValuePair<int, int>> possibleMoves = new List<KeyValuePair<int, int>>();

        for(int i = 0; i < 8; ++i) {
            for(int j = 0; j < 8; ++j) {
                if (board[i][j] == BoardSpace.EMPTY) {
                    BoardSpace enemyColor = turnNumber % 2 == 0 ? BoardSpace.WHITE : BoardSpace.BLACK;
                    BoardSpace ourColor = turnNumber % 2 == 0 ? BoardSpace.BLACK : BoardSpace.WHITE;
                    for (int k = -1; k < 2; ++k) {
                        for (int l = -1; l < 2; ++l) {
                            if (!((k == 0 && l == 0) || k + i < 0 || k + i >= 8 || l + j < 0 || l + j >= 8) && board[k + i][l + j] == enemyColor) {
                                int multiplier = 2;
                                while (k * multiplier + i >= 0 && k * multiplier + i < 8 && l * multiplier + j >= 0 && l * multiplier + j < 8) {
                                    if (board[k * multiplier + i][l * multiplier + j] == BoardSpace.EMPTY) {
                                        break;
                                    } else if (board[k * multiplier + i][l * multiplier + j] == ourColor) {
                                        possibleMoves.Add(new KeyValuePair<int, int>(i, j));
                                        k = 2;
                                        l = 2;
                                        break;
                                    }
                                    ++multiplier;
                                }
                            }
                        }
                    }
                }
            }
        }

        return possibleMoves;
    }

    void GameOver() {
        int blackCount = 0;
        int whiteCount = 0;
        foreach(BoardSpace[] row in board) {
            foreach(BoardSpace space in row) {
                switch (space) {
                    case (BoardSpace.BLACK):
                        blackCount++;
                        break;
                    case (BoardSpace.WHITE):
                        whiteCount++;
                        break;
                }
            }
        }
        //PrintBoardDebug();
        if(blackCount > whiteCount) {
            tText.text = "Black Wins!";
        } else if (blackCount < whiteCount) {
            tText.text = "White Wins!";
        } else {
            tText.text ="Tie";
        }
        
        gameEnded = true;
      
    }

    void PrintBoardDebug() {
        print("printBoard");
        string boardRow = "";
        foreach (BoardSpace[] row in board) {
            foreach (BoardSpace space in row) {
                boardRow += space + "\t";
            }
            print(boardRow);
            boardRow = "";
        }
    }

}
