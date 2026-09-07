# Agentes de IA - Seguridad Y Riesgos

### Seguridad y Riesgos en IA. 
En el pasado, protegíamos software que con todo y bugs era bastante predecible en ejecución. 
Ahora estamos construyendo agentes que interpretan objetivos, deciden pasos intermedios y ejecutan acciones. 

Lo que estamos viendo ahora es un cambio de paradigma en seguridad de software determinista a sistemas agénticos. 
Y este cambio no es solo cosmético, mueve el problema de seguridad de la periferia al centro de la lógica del sistema. 

![Cambio de paradigma en seguridad](./Agentes-IA-Seguridad-Riesgos/Image/CambioEnParadigmaSeguridad/image1.png)

Para este contenido, cuando diga sistema agéntico me refiero al software que no solo responde sino que recibe un objetivo, planifica pasos intermedios, usa herramientas y ejecuta acciones con autonomía acotada por políticas. Esa autonomía acotada es exactamente donde cambian los riesgos. 

![Sistema agéntico](./Agentes-IA-Seguridad-Riesgos/Image/CambioEnParadigmaSeguridad/image2.png)

En el modelo tradicional, el foco estaba en controles conocidos, validar entradas y salidas, autenticar, autorizar y manejar errores. Había incertidumbre, sí, pero el comportamiento era más acotado, eso permitía diseñar pruebas y controles con una cobertura razonable sobre rutas de ejecución esperadas. 

![](./Agentes-IA-Seguridad-Riesgos/Image/CambioEnParadigmaSeguridad/image3.png)

![](./Agentes-IA-Seguridad-Riesgos/Image/CambioEnParadigmaSeguridad/image4.png)

Con agentes, aparece una nueva capa. Hay interpretación de intención, planificación dinámica, uso de herramientas y ajuste por contexto. En ese proceso, surgen riesgos compuestos. 
Por ejemplo, cada paso por separado puede parecer válido, pero la combinación final puede ser insegura. 
Por eso, además de validar código, tenemos que gobernar decisiones y capacidades en tiempo de ejecución. 
Lo que estamos viendo es un cambio del modelo de confianza. 

![](./Agentes-IA-Seguridad-Riesgos/Image/CambioEnParadigmaSeguridad/image5.png)

Con una persona usuaria, la intención suele estar más explícita y hay responsabilidad directa en cada acción. 
Con un agente, delegamos ejecución, entonces la pregunta técnica ya no es solo ¿puede hacer esto? Sino en qué condiciones lo puede hacer, con qué límites y cómo lo detenemos si se desvía. 
Aquí entran controles que vamos a usar durante todo el contenido. Estamos hablando de límites de agencia, privilegio mínimo efectivo, validación de plan antes de ejecutar, trazabilidad de decisión a acción y mecanismos de contención. 
No vamos a reemplazar nunca la seguridad clásica, simplemente vamos a complementarla donde la agencia introduce un riesgo sistémico. Cuando el sistema pasa de ejecutar a decidir, cambia la forma de atacarlo y también la forma de defenderlo.

### Paradigma determinista vs agéntico en la práctica

Vamos a ver en código lo que acabamos de ver en conceptos.

![Tres versiones](./Agentes-IA-Seguridad-Riesgos/Image/CodeExample/image1.png)

Tengo tres versiones del mismo problema, una versión determinista, una versión agéntica sin ningún tipo de control y una versión agéntica con controles. 
La diferencia no está en la cantidad de líneas de código, sino en qué punto el sistema toma las decisiones por sí solo. 
La primera versión es la versión determinista. Como puedes ver, recibe un input, hace una validación y luego ejecuta una query. El flujo siempre sigue el mismo camino, si la validación pasa, hay una sola ruta posible. Eso hace que los controles sean predecibles: valido la entrada, manejo los errores, autorizo. 
Veamos ahora la versión dos, que es la versión agéntica sin ningún tipo de control. Aquí tenemos el mismo objetivo, en este caso estamos hablando de un sistema que ofrece reembolsos. El agente puede ejecutar planes distintos en diferentes corridas, que cuando se está ejecutando ese plan no sabemos exactamente qué es lo que el agente va a definir, digamos. A veces puede hacer una ruta normal en donde va a buscar el pedido y va a encontrar un problema; o a lo mejor va a hacer una ruta más riesgosa donde va a dar un reembolso completo sin ningún tipo de verificación. Como no estamos haciendo ninguna validación sobre este plan, ambas pueden ejecutarse posiblemente. 
Veamos ahora la tercera versión. Esta sigue siendo una versión agéntica, pero hemos añadido una serie de controles. Lo primero que hacemos es validar el objetivo contra la política antes de planificar. Lo siguiente que hacemos es validar si los pasos del plan están permitidos, fíjate que el primero que vamos a hacer es pedirle al agente que ejecute un plan y luego verificar que estén permitidos. Algo que podríamos, por ejemplo, bloquear en este punto es que se haga un reembolso completo sin haber hecho una verificación previa por ejemplo.
Finalmente, lo que vamos a hacer es ejecutar el plan de este agente, pero le vamos a pasar un flag, por ejemplo, `audit=True`, para dejar trazabilidad de qué plan ha sido ejecutado y qué plan no ha sido ejecutado. 
Te voy a mostrar una ejecución de este código, algo que quiero que sepas es que la implementación de este agente está aquí arriba en este archivo, y lo que estamos haciendo aquí es que de forma aleatoria dentro del plan estamos escogiendo diferentes opciones que se pueden hacer como, por ejemplo, dar un reembolso, mirar la política o dar el reembolso sin ningún tipo de chequeo. 
Obviamente, el tipo de agente al que tú estás acostumbrado a escuchar digamos últimamente es aquel que interactúa con un LLM. En este caso, como quiero enfocarme solo en la parte de seguridad, no introduje la parte del LLM y así añadimos esa parte como de inestabilidad o de comportamiento diferente en cada ejecución. 

Si te muestro la ejecución de este programa, lo que tenemos acá abajo,
lo que va a pasar es que se va a hacer una llamada al modelo determinista, una al sistema agéntico sin controles y una al sistema agéntico con controles. 
Vamos a ejecutar esto y vas a ver en este caso que lo que estamos haciendo es que tenemos el pedido 123 y en el modelo determinista la respuesta fue siempre la misma. En el modelo agentico sin controles, lo que se intentó ejecutar aquí fue dar un reembolso, dar un reembolso sin ningún tipo de chequeo, que sabemos que esto no es algo que nos gustaría hacer en este caso.
Y en el tercer caso, donde sí tenemos controles, fíjate que aquí hubo un par de bloqueos porque probablemente se intentó hacer un reembolso sin ningún tipo de chequeo. 

![Ejecución del código](./Agentes-IA-Seguridad-Riesgos/Image/CodeExample/image3.png)

Si volvemos a ejecutar este código, vemos que obtenemos una ejecución diferente, de ahí el tema del comportamiento aleatorio para el ejemplo y vemos que se ha hecho algún tipo de de auditoría, esto es el flag que estábamos pasando de `audit=True`.

![Ejecución del código2](./Agentes-IA-Seguridad-Riesgos/Image/CodeExample/image2.png)

De todas formas, este código lo tienes disponible en el repositorio para que puedas explorarlo por ti mismo.
[Code](Agentes-IA-Seguridad-Riesgos\Agentes-IA-Seguridad-Riesgos\Paradigma.cs)

### Anatomía de un agente en 5 pasos

![Ejecución del código2](./Agentes-IA-Seguridad-Riesgos/Image/5Pasos/image1.png)

A continuación, vamos a analizar los riesgos de forma consistente y vamos a utilizar los cinco pasos del ciclo de vida de un agente. 
Todo empieza cuando entra una solicitud. Si ese objetivo llega ambiguo, conflictivo o manipulable, el resto del flujo nace torcido. 

El primer control es normalizar el objetivo, resolver cualquier tipo de ambigüedad y validarlo contra políticas antes de que el agente empiece a planificar. 
Luego pasamos al proceso de planificación, el agente arma un plan. Aquí el riesgo no es solo un plan malo, sino también la combinación de pasos no autorizados o sin límites. 
El control mínimo que puedes tener aquí es validar ese plan, precondiciones por cada paso y límites de alcance para evitar planes que se expandan solos. 

El tercer paso es el uso de herramientas. Aquí es donde vienen las llamadas a API, funciones o servicios externos. Este punto es crítico porque se pueden utilizar parámetros peligrosos, se pueden hacer acciones irreversibles y se puede usar fuera de contexto. 
El control mínimo que tienes que añadir aquí es una lista permitida de herramientas, validación semántica de los parámetros y privilegio mínimo por llamada. 

Luego pasamos a la parte de memoria o contexto, el agente decide con este contexto. Si el contexto está contaminado o no verificado, la decisión también lo va a estar. El control mínimo que puedes tener aquí es etiquetar confianza de las fuentes, caducar un contexto antiguo y separar memoria confiable de memoria no confiable. 

Finalmente cuando hablamos de la ejecución y la salida, aquí es donde puede haber un riesgo de alto impacto. El control mínimo que puedes hacer aquí es tener políticas por cada nivel de impacto, supervisión humana de decisiones críticas y trazabilidad completa de decisión, acción y resultado.
Por ejemplo, si hay algún tipo de acción crítica, algo que puedes hacer es algo conocido como human-in-the-loop, en donde antes de que el agente ejecute esa acción debe un humano intervenir para aprobarla o rechazarla.

### Los 5 pasos de un agente en la práctica

Permíteme mostrarte cómo se ve este mapa de cinco pasos de un agente en el código. [Code](Agentes-IA-Seguridad-Riesgos\Agentes-IA-Seguridad-Riesgos\PasosAgent1_5.cs)

En este caso tengo dos clases, una se llama AgentUnsafe y la otra se llama AgentSafe. 
La primera ejecuta los cinco pasos sin ningún tipo de control y la segunda tiene exactamente un control por paso. Vamos a mirarlo juntos. 

![class AgentUnsafe](./Agentes-IA-Seguridad-Riesgos/Image/5Pasos/image2.png)

En el paso 1, si vemos aquí el método run del AgentUnsafe, la instrucción que estamos pasando aquí va directamente al plan sin ningún tipo de validación.
Luego, en la planificación, el plan se construye y se usa directamente. No hay ningún tipo de restricción sobre él.
Luego procedemos a usar una lista de herramientas que tenemos aquí guardadas en nuestra variable tools, y aquí el agente puede llamar a cualquier herramienta con cualquier parámetro y, en este caso, el agente unsafe continúa así haya un paso que desconoce. 
En el paso 4, cualquier resultado se guarda como contexto confiable. 
Y en el paso 5, la salida se devuelve directamente al usuario. 

Miremos ahora nuestro AgentSafe como contraste.

![class AgentSafe](./Agentes-IA-Seguridad-Riesgos/Image/5Pasos/image3.png)

Fíjate que aquí lo primero que tenemos son una serie de métodos helpers que vamos a poder utilizar en nuestro método run. Como ves, lo primero que vamos a hacer en nuestro método run es validar el objetivo contra la política del sistema. 
Si esta validación no pasa, el agente no va a llegar ni siquiera a planificar, entonces el riesgo queda bloqueado directamente en este paso. 

El siguiente paso sería planificación, y aquí lo que hacemos es que en el método `assert_plan_within_scope` verificamos el plan contra el scope permitido antes de avanzar. 
Un plan que intenta expandirse más allá de lo autorizado queda detenido aquí. 

Procedemos ahora al uso de herramientas. Aquí vamos a ver que tenemos el método `run_tools`, y si nos metemos aquí hay un `allowed list` explícito, estamos diciendo exactamente qué herramientas puede utilizar este agente. 
Si la herramienta no está en esa lista, la llamada nunca va a ocurrir sin importar lo que el agente haya planeado. 

En el paso número 4, la memoria tiene nivel de confianza. El agente no decide sobre datos no verificados y el contexto viejo no contamina las decisiones futuras. 
Además, estamos añadiendo un tiempo de vida, en este caso, de 24 horas, entonces estamos limitando por cuánto tiempo este agente puede confiar en esta información. Y finalmente tenemos la respuesta. 
Si el impacto es alto, la respuesta espera aprobación humana antes de salir. Lo podemos ver aquí en el método `respond`, vemos que si es de alto impacto vamos a devolver, en este caso, que requiere versión humana y vamos a devolver un error. 
Es decir, que el agente no es el último punto de decisión en acciones de alto impacto. Como bien te comenté en el ejemplo anterior, este ejemplo se enfoca directamente en los pasos de ejecución de un agente y no 100 % en la parte autónoma y de interacción con una LLM de un agente. 

Lo que quiero que entiendas es que, a pesar de que el agente es autónomo y ejecuta un plan y hace todo ese tipo de cosas por su cuenta, todavía nosotros tenemos una serie de controles que podemos ejecutar para que estas ejecuciones sean seguras.