int greenPin = 23;
int bluePin = 22;
int redPin =  21;

byte incomingByte;
boolean handshake;
boolean byteRead;

int ledSelecter = 0;
int ledState = 0;

void setup() {
  Serial.begin(9600);
  pinMode(redPin, OUTPUT);
  pinMode(greenPin, OUTPUT);
  pinMode(bluePin, OUTPUT);
}

void loop() {
  byteRead = false;
  while (Serial.available() > 0)
  {
    incomingByte = Serial.read();
    byteRead = true;
    
    if (incomingByte == 'r')
    {
      ledSelecter = redPin;
      ledState = 100;
      Serial.println("Red set");
    }
    if (incomingByte == 'g')
    {
      ledSelecter = greenPin;
      ledState = 100;
      Serial.println("Green set");
    }
    if (incomingByte == 'b')
    {
      ledSelecter = bluePin;
      ledState = 100;
      Serial.println("Blue set");
    }
    if (incomingByte == 'o')
    {
      ledState = 0;
      Serial.println("LED Off");      
    }
  }
  
  if (byteRead)
  {
    analogWrite(greenPin,0);
    analogWrite(redPin,0);
    analogWrite(bluePin,0);
    if (ledState > 0)
    {
      analogWrite(ledSelecter,ledState);
    }
  }
}

