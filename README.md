# Vuestro juego SIN servidor
¿Es eso posible? Pues la respuesta es que sí. Quizá no sean buenas noticias, teniendo en cuenta las muchas horas de trabajo en el servidor y los quebraderos de cabeza correspondientes. Pero solo así se aprende.     
        
Ahora es el momento de aprender esa sorprendente alternativa, que se explica en este recurso exclusivo que has ganado.     
      
## Un servidor especial
Imagina que existe un servidor que permite a cualquier cliente realizar las siguientes peticiones:         
```           
Subscribeme (“Deportes”)    
```
Le pide al servidor que le envíe una notificación con cualquier información que se publique sobre el tema “Deportes” (o cualquier tema que se indique). El cliente podría subscribirse a todos los temas que quiera.           
```               
Publica (“Deportes”, “El Barça subcampeón”)
```
Le pide al servidor que envíe la noticia a todos los clientes que se han subscrito al tema “Deportes”.            
```
EliminaSubscripción (“Deportes”)        
```
Indica al servidor que no le envíe más noticias sobre el tema “Deportes”.         
      
Imagina también que tenemos la siguiente función:     
```
ParaProcesarNotificacionesIrA (función)      
```             
De esta manera, el cliente le dice al Sistema Operativo qué función debe ejecutar cada vez que llega una notificación del servidor (sea del tema que sea).
Seguro que no te cuesta mucho imaginar un servidor que pueda prestar estos servicios. De hecho, un servidor así apareció en un examen de conocimientos básicos, con el enunciado he incluido en este repositorio.
         
Con un servidor tan sencillo puedes implementar ya una pequeña aplicación de chat. Si María quiere chatear con Juan solo tiene que ejecutar la operación:
```
Subscribeme (“Juan”).      
```       
Naturalmente “Juan” debe hacer:      
```
Subscribeme (“Maria”)     
```    
A partir de ese momento, cuando Maria quiera enviar un mensaje a Juan hará:     
```      
Publica (“Juan”, “Hola Juan”)      
```      
El cliente de Juan recibirá una notificación y el Sistema Operativo le enviará a la función que se le haya indicado para tratar notificaciones. Allí puede mostrar el mensaje de Maria en un cuadro de texto y enviarle una respuesta haciendo:     
```      
Publica (“Maria”, “Hola Maria”)      
```      
Y así hasta que quieran terminar la conversación, momento en el que ejecutarán la operación EliminaSubscripcion.      
      
## El protocolo MQTT
Un sencillo servidor de subscripciones y publicaciones es en realidad una herramienta muy potente y útil. Tanto que se ha definido un protocolo de aplicación para trabajar en ese modo. El protocolo se denomina MQTT (Message Queuing Telemetry Transport). Puedes encontrar más información aquí: https://www.paessler.com/it-explained/mqtt     

Y naturalmente se han desarrollado servidores que implementan este protocolo. El más usado de ellos se llama Mosquitto (te costará poco encontrar más información sobre ese producto).      
    
Los servidores que implementan el protocolo MQTT suelen denominarse brokers. El bróker Mosquitto tiene ese nombre para proyectar la idea de que es un programa muy sencillo y ligero que puede instalarse en procesadores con muy pocos recursos (capacidad de procesador o memoria). Es por tanto ideal para comunicar dispositivos sencillos conectados a internet (por ejemplo, conectar el reloj de pulsera con la nevera y con la lavadora). Es protocolo MQTT es, por tanto, una pieza clave en el mundo del Internet de las Cosas (IOT).     
            
Pero ahora quizá te interese Mosquitto para substituir a vuestro servidor de juego. La pista ya la tienes en el ejemplo del chat. Si dos clientes pueden chatear a través del bróker entonces podrán intercambiara también los movimientos de la partida.     
           
Naturalmente, la cosa es algo más complicada. ¿Cómo mantiene el cliente una lista de conectados? ¿Cómo sería el protocolo de invitación?     
     
## Un chat sin servidor
El código que encontrarás en este repositorio es un formulario en Windows Forms que implementa un chat multijugador (pero no multipartida). El código implementa el mantenimiento de la lista de conectados y el protocolo de invitación de manera que cada cliente ve actualizarse automáticamente la lista de conectados y puede seleccionar a las personas con las que quiere chatear. Allí encontrarás cómo se hacen las operaciones de subscripción y publicación en C#.     
      
También verás una cosa interesante. El código usa un bróker público que implementa MQTT. Así que ni siquiera hace falta que te instales en tu ordenador un bróker mosquitto (aunque puede ser interesante que lo hagas, siguiendo cualquiera de los múltiples tutoriales que hay sobre el tema).      
     
Pues ahí tienes la base de un juego de chat sin escribir ni una sola línea de código de servidor. A partir de ahí no te costará mucho completar el formulario para añadir los gráficos y las mecánicas de vuestro juego porque, como te digo, si el formulario puede intercambiar mensajes de chat con los otros jugadores, también podrá intercambiar jugadas de vuestro juego.     
     
Eso sí, estamos obviando toda la parte de gestión de la base de datos de usuarios y partidas, consultas, etc. Esa es otra historia.      

 
