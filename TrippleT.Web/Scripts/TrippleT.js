TTT = {
    // current player id
    id: "",
    hub: null,
    fieldSize: 4,
    players: {},
    currentScreen: null,
    // is in 3d mode
    d3: true,
    //game state: 
    // 0 - no game
    // 1 - my move
    // 2 - opponent move
    // 3 - end game???
    state: 0,
    clearField: function () {
        var self = this;
        self.field = new Array(self.fieldSize);
        for (var i = 0; i < self.fieldSize; i++) {
            self.field[i] = new Array(self.fieldSize);
            for (var j = 0; j < self.fieldSize; j++) {
                self.field[i][j] = new Array(self.fieldSize);
            }
        }
    },

    startGame: function (opponent, whoFirst) {
        var self = this;
        self.opponent = opponent;
        self.clearField();
        self.showGameScreen();
        self.toggle3d(true); // will generate field
        self.marks = {};
        self.marks[whoFirst] = 1; // first move has X

        if (whoFirst == self.id) { // other player O
            self.marks[opponent] = 2;
            self.myMove();
        }
        else {
            self.marks[self.id] = 2;
            self.opponentMove();
        }

    },
    endGame: function () {
        self.opponent = null;
        self.marks = null;
        self.state = 0;
    },

    myMove: function () {
        this.state = 1;
        $("#move").text("your");
    },
    opponentMove: function () {
        this.state = 2;
        $("#move").text("opponents");
    },


    tryUpdateTile: function (x, y, z, tile) {
        var self = this;
        if (!tile)
            tile = $("#t_" + x + "_" + y + "_" + z);
        if (self.field) {
            var mark = self.field[x][y][z];
            if (mark == 1) { tile.toggleClass("x", true); }
            if (mark == 2) { tile.toggleClass("o", true); }
        }
    },

    toggleMode: function (vertical) {

        var block = $(".block");

        block.toggleClass("span4", vertical);
        block.toggleClass("offset4", vertical);


        block.toggleClass("span14", !vertical);
    },

    toggle3d: function (enable) {
        var self = this;
        if (!enable) {
            enable = !self.d3;
        }

        self.d3 = enable;


        self.toggleMode(self.d3); // go to vertical if 3d and horizontal, if not
        $(".block").toggleClass("block-3d", self.d3);
        
        //HACK: remove and put back lanes of Chrome will 
        // break hover event on top part of plane 
        // see: http://stackoverflow.com/questions/10534697/hover-works-only-on-lower-part-of-rotatex-transformed-div
        $(".plane").remove();
        self.generatefield();



    },

    generatefield: function () {
        var self = this;
        var field = $("#field");
        for (var i = 0; i < self.fieldSize; i++) {
            var p = $('<div class="plane"/>');
            p.toggleClass("d3", self.d3);
            p.css({ top: i * -100 });

            for (var j = 0; j < self.fieldSize * self.fieldSize; j++) {
                var y = Math.floor(j / self.fieldSize);
                var z = j % self.fieldSize;

                var t = $('<div class="tile"/>')
                                    .attr("id", "t_" + i + "_" + y + "_" + z)
									.data({ x: i, y: y, z: z })
									.click(function () { self.tileClick(this); })
									.appendTo(p);
                self.tryUpdateTile(i, y, z, t);
            }
            p.appendTo(field);

            $('<div style="clear:both;"/>').appendTo(p);
        }
    },

    tileClick: function (tile) {
        var self = this;
        if (self.state == 1) // my move
        {
            var pos = $(tile).data();
            if (self.field[pos.x][pos.y][pos.z]) return;
            self.hub.move(pos.x, pos.y, pos.z);

        }
    },

    updateMove: function (id, x, y, z) {

        var self = this;

        // do i realy need this check?
        if (self.opponent != id && this.id != id) return;

        self.field[x][y][z] = this.marks[id];
        self.tryUpdateTile(x, y, z);
        if (self.opponent == id)// i was oppoonent's move, no just update of yours
            self.myMove();
        if (self.id == id)
            self.opponentMove();
    },

    showScreen: function (screen, callback) {
        var self = this;
        if (self.currentScreen) {
            self.currentScreen.toggle(false);
            if (self.currentScreenCallback) {
                self.currentScreenCallback(screen);
            }
        }
        screen.toggle(true);
        self.currentScreen = screen;
        self.currentScreenCallback = callback;
    },

    showPlayers: function () {
        var self = this;
        $("#myName").text(self.name);
        self.showScreen(self.playersScreen);
    },

    showGameScreen: function () {
        var self = this;
        self.showScreen(self.gameScreen, function () {
            self.toggleMode(true);
            $(".block").toggleClass("block-3d", false);
        });
    },

    showOffer: function (id) {
        var self = this;

        $("#offerName").text(self.players[id]);
        self.state = 4;
        self.offerBox.toggle(true);
    },

    showWin: function (id) {
        var self = this;
        self.endGame();
        $("#winner").text(self.players[id]);
        self.showScreen(self.victoryScreen);
    },

    showExit: function (id) {
        var self = this;
        self.endGame();
        self.showScreen(self.exitScreen);
    },

    playerClick: function (id) {
        var self = this;
        if (id != self.id)
            self.hub.makeOffer(id);
    },

    init: function () {
        var self = this;

        self.loginScreen = $("#loginScreen");
        self.playersScreen = $("#playersScreen");
        self.gameScreen = $("#gameScreen");
        self.victoryScreen = $("#victoryScreen");
        self.exitScreen = $("#exitScreen");
        self.offerBox = $("#offerBox");

        var connection = $.connection;
        var hub = connection.tickTacToe;


        hub.Enter = function (id, name) {

            if (self.players[id]) return;

            self.players[id] = name;

            if (id == self.id)// we are in!
            {
                self.name = name;
                self.showPlayers();
            }
            else {
                $("#noOne").remove();
            }

            $("<li>")
                .append($('<a href="#"> ')
                        .click(function () { self.playerClick(id); })
                        .attr("id", "pl_" + id)
                        .text(name)).appendTo($("#playerList"));
        };
        hub.Left = function (id) {
            $("#pl_" + id).remove();
            if (id == self.opponent) {
                self.showExit(id);
            }
        };
        hub.LeftGame = function (id) {
            if (id == self.opponent) {
                self.showExit(id);
            }
        };

        hub.Game = function (ida, idb) {
            if (ida == self.id) {
                self.startGame(idb, ida);
            }
            if (idb == self.id) {
                self.startGame(ida, ida);
            }
        };

        hub.Win = function (id) {
            self.showWin(id);
        };

        hub.Offer = function (id) {
            self.showOffer(id);
        };

        hub.UpdateMove = function (id, x, y, z) {
            self.updateMove(id, x, y, z);
        };

        connection.hub.start().then(function () {
            self.id = connection.hub.id;
        });

        $("#acceptOffer").click(function () {
            hub.accept(); $(this).parent().toggle();
        });
        $("#declineOffer").click(function () {
            hub.accept(); $(this).parent().toggle();
        });

        $("#enterButton").click(function () {
            hub.register($("#playerName").val());
        });
        $("#leaveGame").click(function () {
            hub.leaveGame();
            self.endGame();
            self.showPlayers();
        });
        $(".backButton").click(function () {
            self.showScreen(self.playersScreen);
        });

        self.showScreen(self.loginScreen);
        self.hub = hub;
    }

};
