var typingTimer;               //timer identifier
var doneTypingInterval = 200;  //time in ms, 5 second for example
var $trainingArea = $('#training-input');
var $input = $('#testing-input');
var $levelNum = $('#markov-level');

//on keyup, start the countdown
$input.on('keyup', function () {
    if ($input.is(':focus')){
        clearTimeout(typingTimer);
        typingTimer = setTimeout(GetNextPredictions, doneTypingInterval);
    }
    else{
        console.log('not focused');
    }
});

//on keydown, clear the countdown 
$input.on('keydown', function () {
    if ($input.is(':focus')) {
        clearTimeout(typingTimer);
    }
    else {
        console.log('not focused');
    }
});

$("#suggestion-1").on('click', function () {
    insertChoice($("#suggestion-1"));
});

$("#suggestion-2").on('click', function () {
    insertChoice($("#suggestion-2"));
});

$("#suggestion-3").on('click', function () {
    insertChoice($("#suggestion-3"));
});

function insertChoice($element) {
    var currentText = $input.val();
    if ($element.attr('name') !== '-') { //get value of element by class
        var newText = currentText.concat("\r\n", $element.attr('name'));
        console.log('Setting new text: '.concat(newText));
        $input.val(newText);

        GetNextPredictions();
    }
}

function Train() {
    var level = $levelNum.val();
    console.log('training model with level ' + level);
    var txt = $trainingArea.val();

    var trainingPostData = {
        modelLevel: level,
        trainingData: txt
    };
    console.log(trainingPostData);

    jQuery.ajax({ //calling <<the HomeController.cs function><YKWIM>>
        url: "/Home/Train", //the function in HomeController.cs? <<Seeing such as with "[HttpPost]"><YKWIM>>
        type: "POST",
        data: JSON.stringify(trainingPostData), //passing the data to HomeController.cs as JSON
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            console.log(data.Message); //data.Message being the returned ActionResult and\or such ...
            GetNextPredictions();
        }
    });
}

//user is finished typing, get predictions
function GetNextPredictions() { //For the text in the <<"predictions" textarea><YKWIM>> ...retrieve "predictions" based on seedText and model <wtl: <<- where I was going to put the most immediately following "at least" text within maybe some angled bracketed writing such as of <rmh: "YKWIM>what would be with corresponding <rmh: "YKWIM>"<YKWIM>" writing<wtl:  - >><-nmn>><rmth: <wtl:  - >>><at least> and return what would be the top 3 'next samples' based on those samples found in descending order of 'most instances of such samples as seen before'
    //For the game, could make use of the prevalences and\or such of each of the 'next samples' and use such as probabilities for the next sequence that would be used?
    console.log('getting predictions');

    var seedText = $input.val();

    var postBody = {
        seedText: seedText
    };
    console.log(postBody);

    jQuery.ajax({
    //send POST request data to the GetPredictions function in the "#suggestion-[num]" fields <<and\or such><YKWIM>>
        url: "/Home/GetPredictions",
        type: "POST",
        data: JSON.stringify(postBody),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            console.log(data);
            console.log(data.SuggestionsCount[0]);
            console.log(data.SuggestionsTotalCount);
            //assign both a label for the button, and a name where the name would be the data copied to the resulting textarea
            $("#suggestion-1").html(data.Suggestions[0] != undefined ? data.Suggestions[0].concat(" (", ((data.SuggestionsCount[0]) * 1.0 / data.SuggestionsTotalCount), ")") : '-');
            $("#suggestion-1").attr("name", data.Suggestions[0] || '-');
            //$("#suggestion-1").name(data.Suggestions[0] || '-');

            $("#suggestion-2").html(data.Suggestions[1]!=undefined?data.Suggestions[1].concat(" (", ((data.SuggestionsCount[1]) * 1.0 / data.SuggestionsTotalCount), ")"):'-');

            //--------------------------
            //Regarding such done here: 
            //Testing to see if there would be an error resulting from the concat function use as what would have led to the lack of changing button data <wtl: <<>and\or such<wtl: ><YKWIM>>>
            //noting such in finding errors and\or such and narrowing such down regarding such Javascript as what would done here, regarding lack of value changes and\or such of the buttons and any corresponding numbers as seen here
            //and noting such in eg. use of getting the Markov Model to function as I would consider such ...
            //and <<testing such as would have been suggested by atsi regarding using such as sequences><YKWIM>> and getting such to work in such a way ... at first at least ... ensuring such as functioning as intended and\or such and seeing if such would be what I would want, with eg. resulting output and\or such ... and using such information and\or such ...
            //Noting any of such and being unsuccess and FWIS in getting such to fully work in such a way as well ... in noting Etime in doing such and\or such ...
            //--------------------------

            //console.log("Suggestions[1]: ");
            //console.log(data.Suggestions[1]);
            //$("#suggestion-2").html(data.Suggestions[1].concat(" (", ((data.SuggestionsCount[1]) * 1.0 / data.SuggestionsTotalCount), ")") || '-');
            ////$("#suggestion-2").className = '';
            //console.log(data.Suggestions[1]);
            //data.Suggestions[1].concat(""); //see if this would make Suggestions[1] no longer undefined? //well, led to an error
            $("#suggestion-2").attr("name", data.Suggestions[1] || '-');

            $("#suggestion-3").html(data.Suggestions[2]!=undefined?data.Suggestions[2].concat(" (", ((data.SuggestionsCount[2]) * 1.0 / data.SuggestionsTotalCount), ")"):'-');
            //$("#suggestion-3").className = '';
            $("#suggestion-3").attr("name", data.Suggestions[2] || '-');
        }
    });
}