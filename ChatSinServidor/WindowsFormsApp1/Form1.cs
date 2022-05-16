using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        List<string> invitados = new List<string>();
        MqttClient mqttClient;
        Random random = new Random();
        string clave;
        Boolean conectado;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            groupBox1.Visible = false;
            dataGridView1.ColumnCount = 1;
            button3.Visible = false;
            conectado = false;
        }

        private void AtenderNotificacion(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        // Esta es la función que se ejecuta cuando recibo una notificación del broker
        {
            if (e.Topic.Contains ("conectado"))
            // Alguien se ha conectado. En el mensaje viene su nombre
            {
                Boolean esta = false;
                string nombreConectado = Encoding.UTF8.GetString(e.Message);
                // Lo añado a la tabla
                dataGridView1.Invoke(new Action(() =>
                {
                    dataGridView1.Rows.Add(nombreConectado);
                    dataGridView1.ClearSelection();
                 }));
                 // Le indico al que se acaba de conectar que yo también estoy conectado para que actualice su lista de conectados
                 mqttClient.Publish("yaConectado/" + nombreConectado, Encoding.UTF8.GetBytes(nombreBox.Text), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
     
            }
            if (e.Topic.Contains("yaConectado"))
            // Me acabo de conectar y alguien me indica que ya está conectado
            {
                string nombreConectado = Encoding.UTF8.GetString(e.Message);
                // Si no soy yo mismo lo añado a la lista de conectados
                if (nombreConectado != nombreBox.Text)
                {
                   
                        dataGridView1.Invoke(new Action(() =>
                        {
                            dataGridView1.Rows.Add(nombreConectado);
                            dataGridView1.ClearSelection();
                        }));
                    
                }
            }
            if (e.Topic.Contains("invitacion"))
            {
                // Alguien me invita. El mensaje contiene una clave y el nombre del que me invita, separados por '/'
                string[] trozos = Encoding.UTF8.GetString(e.Message).Split('/');

                clave = trozos[0];
                string anfitrion = trozos[1];
                DialogResult dialogResult = MessageBox.Show("Aceptas la invitacion de: " + anfitrion, "Invitación", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                // Si acepto la invitación abro la zona de chateo
                {
                    groupBox1.Invoke(new Action(() =>
                    {
                        groupBox1.Visible = true;
                    }));
                    // Y me subscribo a cualquier notificación con mensajes o teminación del chat con esa clave
                    mqttClient.Subscribe(new string[] { "mensaje/" + clave }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                    mqttClient.Subscribe(new string[] { "terminar/" + clave }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

                }
            }
            if (e.Topic.Contains("mensaje"))
            // Recibo un mensaje del chat en el que participo
            {
                string mensaje = Encoding.UTF8.GetString(e.Message);

                listBox1.Invoke(new Action(() =>
                {
                    listBox1.Items.Add(mensaje);
                }));
            }
            if (e.Topic.Contains("terminar"))
            // Termina el chat en el que participo
            {
                // Abandono las subscripciones a ese chat
                mqttClient.Unsubscribe(new string[] { "mensaje/" + clave, "terminar/" + clave });
                // Oculto el cuadro de chat
                groupBox1.Invoke(new Action(() =>
                {
                    listBox1.Items.Clear();
                    groupBox1.Visible = false;
                }));

            }
            if (e.Topic.Contains("desconectar"))
            // Alguien se desconecta. Su nombre está en el mensaje
            {
                if (nombreBox.Text != Encoding.UTF8.GetString(e.Message))
                // si no soy yo busco en la lista de conectados y lo elimino
                {
                    dataGridView1.Invoke(new Action(() =>
                    {
                        int rowIndex = 0;
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (row.Cells[0].Value.ToString().Equals(Encoding.UTF8.GetString(e.Message)))
                            {
                                rowIndex = row.Index;
                                break;
                            }
                        }
                        dataGridView1.Rows.RemoveAt(rowIndex);
                        dataGridView1.Refresh();
                    }));
                }
            }
        }



        private void button1_Click_1(object sender, EventArgs e)
        {
            // Envio un mensaje al chat en el que participo. Tengo guardada la clave de ese chat
            // Envio mi nombre y el mensaje
            string mensaje = nombreBox.Text + ": " + textBox1.Text;
            mqttClient.Publish("mensaje/" + clave, Encoding.UTF8.GetBytes(mensaje), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            textBox1.Text = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Me conecto al broker indicando mi nombre (si ni estoy ya conectado)
            if (!conectado)
            {
                // Unsamos un broker público y gratuito
                mqttClient = new MqttClient("broker.hivemq.com");
                mqttClient.Connect(nombreBox.Text);

                // indico qué función quiero ejecutar cuando reciba una notificación
                mqttClient.MqttMsgPublishReceived += AtenderNotificacion;
            }
          
            // Quiero recibir notificaciones cuando:
            // Alguien se conecta
            mqttClient.Subscribe(new string[] { "conectado" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            // Cuando me conecto los que ya están conectados me enviarán una notificación para que actualice la lista de conectados
            // Me subscribo a este tipo de notificaciones
            mqttClient.Subscribe(new string[] { "yaConectado/" + nombreBox.Text }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            // Alguien se desconecta
            mqttClient.Subscribe(new string[] { "desconectar" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            // Alguien me invita
            mqttClient.Subscribe(new string[] { "invitacion/" + nombreBox.Text }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

            // Ahora aviso de que me he conectado
            mqttClient.Publish("conectado", Encoding.UTF8.GetBytes(nombreBox.Text), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

            button3.Visible = true;
            button2.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Me desconecto
            // Abandono las subscripciones que tengo
            mqttClient.Unsubscribe(new string[] { "conectado", "yaConectado/" + nombreBox.Text, "desconectar", "invitacion/" + nombreBox.Text });
            // Indico que me desconecto indicando mi nombre en el mensaje
            mqttClient.Publish("desconectar", Encoding.UTF8.GetBytes(nombreBox.Text), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

            dataGridView1.Rows.Clear();
            int cont = dataGridView1.Rows.Count;
            //En buena lógica ahora deberia desconectarme del broker haciendo mqttClient.Disconnect();
            //Si lo hago aqui el mensaje de desconexión no llega al resto. 
            //No pasa nada si no me desconecto. Asi ya estoy conectado si el usuario quiere volver a conectarse
           
            button2.Visible = true;
            button3.Visible = false;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        // Selecciono un nuevo invitado
        {
            string invitado = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
            // Si no está en la lista de personas a las que voy a invitar lo añado
            if (invitados.Contains(invitado))
            {
                invitados.Remove(invitado);
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            }
            else
            // y si está lo quito
            {
                invitados.Add(invitado);
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Green;

            }
            dataGridView1.ClearSelection();

        }
       

        private string RandomString(int length)
        {
            // Genero una clave aleatoria alfanumerica de length caracteres
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void button4_Click(object sender, EventArgs e)
        // Voy a invitar a las personas seleccionadas
        {
            // Genero la clave alfanumérica para el chat que vamos a iniciar (la guardo en variable pública)
            clave = RandomString(8);
            foreach (string invitado in invitados)
            {
                // A cada seleccionado le envio la invitación con la clave y mi nombre
                string invitacion = clave + '/' + nombreBox.Text;
                mqttClient.Publish("invitacion/" + invitado, Encoding.UTF8.GetBytes(invitacion), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

            }
            invitados.Clear();
            groupBox1.Visible = true;
            // Me subscribo a cualquier notificación de ese chat (mensajes o terminación del chat)
            mqttClient.Subscribe(new string[] { "mensaje/" + clave }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            mqttClient.Subscribe(new string[] { "terminar/" + clave }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.DefaultCellStyle.BackColor = Color.White;
            }
        }

  
        private void button5_Click(object sender, EventArgs e)
        {
            // doy por terminado el chat
            mqttClient.Publish("terminar/" + clave, Encoding.UTF8.GetBytes(""), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            listBox1.Items.Clear();
        }
    }
}
