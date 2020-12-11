﻿function UpdateCount() {
    $.ajax({
        url: "/storage/GetTotalNumber",
        type: "GET",
        success: function (data) {
            $("#displayCount").empty();
            $("#displayCount").append(data);
        }
    });
}

//function AddOne(value) {
//    $.ajax({
//        url: "/storage/AddNumber",
//        type: "GET",
//        data: { 'value': value },
//        success: function () {
//        }
//    });
//}

function DDOS(hubConnection) {
    var time = performance.now();

    for (let i = 0; i < 100000; i++) {
        hubConnection.invoke("AddValue", '1');
    }

    console.log('Время выполнения = ', performance.now() - time);
}


$(document).ready(function () {
    hubConnection = new signalR.HubConnectionBuilder()
        .withUrl("/counter")
        .build();

    hubConnection.on("Send", function (data) {
        $("#displayCount").empty();
        $("#displayCount").append(data);
    });

    document.getElementById("sendBtn").addEventListener("click", function (e) {
        //let message = document.getElementById("message").value;
        hubConnection.invoke("AddValue", '1');
    });

    hubConnection.start();

    setInterval(UpdateCount, 200);

    setTimeout(() => { DDOS(hubConnection); }, 5000);
});