let selectedExercise = '';
let selectedExerciseIndex = -1;
let exerciseIDs = [];
let exerciseProgress = {};
let elements = document.getElementsByClassName('mpExerciseTitle');

let getExerciseElements = function () {
    let exercises = document.getElementById('exercises').children;

    let res = [];

    for (let j = 0; j < exercises.length; j++) {
        if (exercises[j].id != 'exercisesTitle') {
            res.push(exercises[j]);
        }
    }

    return res;
}

let deselectExercises = function () {
    let list = document.getElementById('sets').children;

    for (let j = 0, len = list.length; j < len; j++) {
        if (list[j].id != 'setsTitle') list[j].style.display = 'none';
    }

    document.getElementById('setsTitle').innerText = 'Pick an exercise ... ';

    let exercises = getExerciseElements();

    for (let j = 0; j < exercises.length; j++) {
        if (isExerciseDone(exerciseIDs[j])) exercises[j].className = 'mpExerciseTitleDone';
        else exercises[j].className = 'mpExerciseTitle';
    }

    selectedExercise = '';
    selectedExerciseIndex = -1;
};

let selectExercise = function (id) {
    deselectExercises();

    document.getElementById('setsTitle').innerText = document.getElementById(id).innerText;

    let ele = document.getElementById(id);

    if (!ele) {
        deselectExercises();
        return;
    }

    ele.className = 'mpExerciseTitleSelected';

    let index = 0;
    let infoEle = document.getElementById(id + '_setinfo_' + index);
    while (infoEle) {
        infoEle.style.display = '';
        if (exerciseProgress[id] > index) {
            infoEle.className = 'mpExerciseSetDone'
        } else if (exerciseProgress[id] == index) {
            infoEle.className = 'mpExerciseSetSelected'
        } else {
            infoEle.className = 'mpExerciseSet'
        }

        infoEle = document.getElementById(id + '_setinfo_' + ++index);
    }

    selectedExercise = id;
    selectedExerciseIndex = findExerciseIndexByID(id);
};

let selectExerciseByIndex = function (index) {
    if (index < 0 || index >= exerciseIDs.length) {
        console.log("negative exercise index, deselecting");
        deselectExercises();
    } else {
        selectExercise(exerciseIDs[index]);
    }
};

let findExerciseIndexByID = function (id) {
    for (let i = 0; i < exerciseIDs.length; i++) {
        if (exerciseIDs[i] == id) return i;
    }
    return -1;
};

let isExerciseDone = function (id) {
    return !document.getElementById(id + '_setinfo_' + exerciseProgress[id])
}

let getNextIncompleteExercise = function () {
    for (let i = 0; i < exerciseIDs.length; i++) {
        if (!isExerciseDone(exerciseIDs[i])) return i;
    }
    return -1;
}

//set up state
for (let i = 0, len = elements.length; i < len; i++) {
    let ele = elements[i];
    let id = ele.id;

    if (id != 'exercisesTitle') {

        exerciseProgress[id] = 0;
        exerciseIDs.push(id);

        ele.onclick = function () {
            if (ele.className != 'mpExerciseTitleSelected') {
                ele.className = 'mpExerciseTitleSelected';

                selectExercise(id);
            } else {
                deselectExercises();
                ele.className = 'mpExerciseTitle';
            }
        }
    }
}


document.getElementById('nextExercise').onclick = function () {
    if (selectedExercise != '') {

        //check if this is the final one, and if so, advance to the next exercise
        if (!isExerciseDone(selectedExercise)) {
            if (document.getElementById(selectedExercise + '_setinfo_' + exerciseProgress[selectedExercise])) {
                exerciseProgress[selectedExercise]++;
                if (!document.getElementById(selectedExercise + '_setinfo_' + (exerciseProgress[selectedExercise]))) {
                    selectExerciseByIndex(getNextIncompleteExercise());
                } else {
                    selectExercise(selectedExercise);
                }
            }
        } else {
            if(selectedExerciseIndex != exerciseIDs.length - 1) selectExerciseByIndex(getNextIncompleteExercise());
        }
    }
}


deselectExercises();