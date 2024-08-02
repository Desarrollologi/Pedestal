// Ocultar el mensaje después de 5 segundos
setTimeout(function () {
    var messageElement = document.getElementById("confirmation-message");
    if (messageElement) {
        messageElement.style.display = "none";
    }
}, 5000); // 5000 ms = 5 segundos