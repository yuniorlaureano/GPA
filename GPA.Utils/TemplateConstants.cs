namespace GPA.Utils
{
    public class TemplateConstants
    {
        public static string TransactionTemplate()
        {
            return """ 
<html lang="en">
  <head>
<meta charset="UTF-8" />
    <style>
      table {
        color: #242424;
        font-size: 0.85rem;
        border: solid 1px #e6e9ed;
        width: 100%;
        margin-bottom: 1rem;
        vertical-align: top;
        border-color: #dee2e6;
        caption-side: bottom;
        border-collapse: collapse;
        box-sizing: border-box;
        display: table;
        border-collapse: separate;
        box-sizing: border-box;
        text-indent: initial;
        unicode-bidi: isolate;
        border-color: gray;
        padding: 0;
        border-spacing: 0;
      }

      th,
      td {
        text-align: left;
        padding: 8px;
        border: solid 1px #f5f7f8;
        vertical-align: bottom;
        margin: 0;
      }

      th {
        background-color: #f8f9fa;
      }

      td {
        vertical-align: top;
      }
      .content:nth-child(odd) {
        background-color: #f5f4f4;
      }

      .header {
        background-color: #f8f9fa;
        font-weight: 400 !important;
        font-size: 17px;
      }
    </style>
  </head>
  <body>
    <h1 style="text-align: center">Transacciones</h1>
    <table>
      <tr class="header">
        <th style="width: 70px">Estado</th>
        <th style="width: 80">Operación</th>
        <th style="width: 60">Fecha</th>
        <th>Proveedor</th>
        <th>Motivo</th>
        <th>Descripción</th>
        <th>Realizada por</th>
        <th>Actualizada por</th>
      </tr>
      {{Content}}
    </table>
  </body>
</html>
""";
        }

        public static string SaleTemplate()
        {
            return """ 
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <style>
      table {
        width: 100%;
        border-collapse: collapse;
      }

      .inner-table td,
      th {
        border: 1px solid black;
        padding: 8px;
      }
    </style>
  </head>
  <body>
    <table>
      <tr>
        <td>
          <img
            src="data:image/png;base64,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"
            alt=""
            width="150"
          />
        </td>
        <td style="text-align: center">
          <h1>DE AGUA UNAD</h1>
          <h3>Reporte de venta</h3>
        </td>
        <td style="text-align: right; vertical-align: bottom">
          {{Date}}
        </td>
      </tr>
      <tr style="border-bottom: solid 1px black; border-top: solid 1px black">
        <td>Desde: {{From}}</td>
        <td>Hasta: {{To}}</td>
        <td></td>
      </tr>
      <tr>
        <td colspan="3" style="margin-top: 4px">
          <table class="inner-table" style="margin-top: 5px">
            <tr>
              <th>Código</th>
              <th>Fecha</th>
              <th>Cliente</th>
              <th>Método de pago</th>
              <th>Monto</th>
            </tr>
            {{Content}}
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>
""";
        }

        public static string ExistenceTemplate()
        {
            return """ 
<html lang="en">
  <head>
<meta charset="UTF-8" />
    <style>
      table {
        color: #242424;
        font-size: 0.85rem;
        border: solid 1px #e6e9ed;
        width: 100%;
        margin-bottom: 1rem;
        vertical-align: top;
        border-color: #dee2e6;
        caption-side: bottom;
        border-collapse: collapse;
        box-sizing: border-box;
        display: table;
        border-collapse: separate;
        box-sizing: border-box;
        text-indent: initial;
        unicode-bidi: isolate;
        border-color: gray;
        padding: 0;
        border-spacing: 0;
      }

      th,
      td {
        text-align: left;
        padding: 8px;
        border: solid 1px #f5f7f8;
        vertical-align: bottom;
        margin: 0;
      }

      th {
        background-color: #f8f9fa;
      }

      td {
        vertical-align: top;
      }
      .content:nth-child(odd) {
        background-color: #f5f4f4;
      }

      .header {
        background-color: #f8f9fa;
        font-weight: 400 !important;
        font-size: 17px;
      }
    </style>
  </head>
  <body>
    <h1 style="text-align: center">Transacciones</h1>
    <table>
      <tr class="header">
        <th>Código</th>
        <th>Producto</th>
        <th>Tipo</th>
        <th>Entrada</th>
        <th>Salida</th>
        <th>Existencia</th>
        <th>Monto en producto</th>
      </tr>
      {{Content}}
    </table>
  </body>
</html>
""";
        }

        public static string StockDetailsTemplate()
        {
            return """ 
<html lang="en">
  <head>
<meta charset="UTF-8" />
    <style>
      .initial-detail {
        margin-right: 15px;
        color: blue;
        border-right: solid 1px red;
        padding-right: 10px;
      }

      .final-detail {
        color: green;
        margin: 0;
        padding: 0;
      }

      .initial-indicator {
        padding: 10px;
        background: blue;
        width: 1px;
        border-radius: 50%;
        display: inline-block;
        margin-right: 5px;
      }

      .final-indicator {
        padding: 10px;
        background: green;
        width: 1px;
        border-radius: 50%;
        display: inline-block;
        margin-left: 10px;
      }

      .invoice-num {
        text-align: right;
        padding-top: 5px;
      }

      table {
        color: #242424;
        font-size: 0.85rem;
        border: solid 1px #e6e9ed;
        width: 100%;
        margin-bottom: 1rem;
        vertical-align: top;
        border-color: #dee2e6;
        caption-side: bottom;
        border-collapse: collapse;
        box-sizing: border-box;
        display: table;
        border-collapse: separate;
        box-sizing: border-box;
        text-indent: initial;
        unicode-bidi: isolate;
        border-color: gray;
        padding: 0;
        border-spacing: 0;
      }

      th,
      td {
        text-align: left;
        padding: 8px;
        border: solid 1px #f5f7f8;
        font-weight: 400;
        vertical-align: bottom;
        margin: 0;
      }

      th {
        background-color: #f8f9fa;
      }

      td {
        vertical-align: top;
      }
    </style>
  </head>
  <body>
    <div class="invoice-container">
      <div>
        <span style="font-weight: bold">Fecha de apertura:</span>
        {{StartDate}}
        <br />
        <span style="font-weight: bold">Fecha de cierre:</span>
        {{EndDate}} <br />
        <span style="font-weight: bold">Estado:</span>
        {{Status}}
        <br />
        <span style="font-weight: bold">Nota:</span>
        {{Note}} <br />
        <div class="invoice-num">
          <span class="initial-indicator"></span> Apertura
          <span class="final-indicator"></span> Cierre
        </div>
      </div>
      <hr />
      <table>
        <tr>
          <th>Producto</th>
          <th>Código</th>
          <th>Precio</th>
          <th>Entraron</th>
          <th>Salieron</th>
          <th>En existencia</th>
        </tr>
        {{Content}}        
      </table>
    </div>
  </body>
</html>
""";
        }

        public static string InvoiceTemplate()
        {
            return """
                <!DOCTYPE html>
                <html lang="en">
                  <head>
                    <meta charset="UTF-8" />
                    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                    <title>Invoice</title>
                    <style>
                      html {
                  font-family: Verdana, Geneva, Tahoma, sans-serif;
                  margin: 0;
                  padding: 0;
                  font-size: 12px;
                  display: flex;
                  justify-content: center;
                  flex-direction: column;
                }

                          body {
                  margin: 5px;
                  padding: 0;
                }

                          table {
                  width: 100%;
                }

                table td {
                  padding: 5px;
                  vertical-align: top;
                }
                table tr td:nth-child(2) {
                  text-align: right;
                }
                .qr-code {
                  text-align: center;
                  margin-top: 20px;
                }

                /* invoice header */
                .invoce-header {
                  text-align: center;
                }
                .mb-9 {
                  margin-bottom: 9px;
                }

                .receit-title {
                  font-size: 20px;
                  font-weight: bold;
                  align-items: center;
                }

                .bold {
                  font-weight: bold;
                }

                .m0 {
                  margin: 0;
                }
                .p0 {
                  padding: 0;
                }

                /* details */
                table.details th,
                td {
                  margin: 0 !important;
                  padding: 0 !important;
                }
                    </style>
                  </head>
                  <body>
                      <table>
                        <tr>
                          <td class="title">
                            <center>
                              <img
                                src="{Logo}"
                                style="width: 200px"
                              />
                            </center>
                          </td>
                        </tr>
                        <tr>
                          <td class="invoce-header">
                            <div class="mb-9 receit-title">{Company}</div>
                            <div class="mb-9">{Document}</div>
                            <div class="mb-9">{Tel}</div>
                            <div class="mb-9">{Mail}</div>
                            <div>
                                {Address}
                            </div>
                          </td>
                        </tr>
                      </table>
                      <table class="details">
                        <tr>
                          <td colspan="2">
                            <center>
                              ------------------------------------------------------
                            </center>
                          </td>
                        </tr>
                        <tr>
                          <td>Usuario: {User}</td>
                          <td></td>
                        </tr>
                        <tr>
                          <td>Hora.:</td>
                          <td>{Hour}</td>
                        </tr>
                        <tr>
                          <td>Fecha.:</td>
                          <td>{Date}</td>
                        </tr>
                        <tr>
                          <td colspan="2">
                            <center>
                              ------------------------------------------------------
                            </center>
                          </td>
                        </tr>
                        <tr>
                          <td colspan="2">PRODUCTOS</td>
                        </tr>
                        {Products}
                        <tr>
                          <td colspan="2">
                            <center>
                              ------------------------------------------------------
                            </center>
                          </td>
                        </tr>
                        <tr>
                          <td>Total</td>
                          <td></td>
                        </tr>
                        {Totals}
                        <tr>
                          <td colspan="2">
                            <center>
                              ------------------------------------------------------
                            </center>
                          </td>
                        </tr>
                        <tr>
                          <td></td>
                          <td>{TotalPrice}</td>
                        </tr>
                      </table>
                      <div class="qr-code">
                        <img
                          style="width: 100px"
                          src="{QrCode}"
                          alt="QR Code"
                        />
                      </div>
                  </body>
                </html>
                """;
        }

        public static string ProofOfPaymentTemplate()
        {
            return """
                <!DOCTYPE html>
                <html lang="en">
                  <head>
                    <meta charset="UTF-8" />
                    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                    <title>Invoice</title>
                    <style>
                      html {
                        font-family: Verdana, Geneva, Tahoma, sans-serif;
                        margin: 0;
                        padding: 0;
                        font-size: 12px;
                        display: flex;
                        justify-content: center;
                        flex-direction: column;
                      }

                      table {
                        width: 100%;
                      }
                      body {
                        margin: 5px;
                        padding: 0;
                      }

                      table tr td:nth-child(2) {
                        text-align: right;
                      }
                      .qr-code {
                        text-align: center;
                        margin-top: 20px;
                      }

                      /* invoice header */
                      .invoce-header {
                        text-align: center;
                      }
                      .mb-9 {
                        margin-bottom: 9px;
                      }

                      .receit-title {
                        font-size: 20px;
                        font-weight: bold;
                        align-items: center;
                      }

                      .bold {
                        font-weight: bold;
                      }

                      .m0 {
                        margin: 0;
                      }
                      .p0 {
                        padding: 0;
                      }

                      /* details */
                      table.details th,
                      td {
                        margin: 0 !important;
                        padding: 0 !important;
                      }
                    </style>
                  </head>
                  <body>
                    <table>
                      <tr>
                        <td class="title">
                          <center>
                            <img
                              src="{Logo}"
                              style="width: 200px"
                            />
                          </center>
                        </td>
                      </tr>
                      <tr>
                        <td class="invoce-header">
                          <div class="mb-9 receit-title">{Company}</div>
                          <div class="mb-9">{Document}</div>
                          <div class="mb-9">{Tel}</div>
                          <div class="mb-9">{Mail}</div>
                          <div>{Address}</div>
                          <div>------------------------------------------------------</div>
                          <b class="bold">RECIBO DE PAGO</b>
                          <div>{Date}</div>
                          <div>------------------------------------------------------</div>
                        </td>
                      </tr>
                    </table>
                    <table class="details">
                      <tr>
                        <th>Recibí de:</th>
                        <td>{Client}</td>
                      </tr>
                      <tr>
                        <td colspan="2">
                          <center>
                            ------------------------------------------------------
                          </center>
                        </td>
                      </tr>
                      {Amounts}
                      <tr>
                        <td colspan="2">
                          <center>
                            ------------------------------------------------------
                          </center>
                        </td>
                      </tr>
                      <tr>
                         <th>Total</th>
                         <td>{Total}</td>                        
                      </tr>

                    </table>
                    <div>
                      <br />
                      <br />
                      <div class="bold">Concepto:</div>
                      <div style="margin-left: 20px">{Concept}</div>

                      <br />
                      <br />
                      <br />
                      <br />
                      <div style="border-top: solid 1px black; text-align: center">
                        {Signer}
                      </div>
                    </div>
                  </body>
                </html>                
                """;
        }

        public static string ReceivableProofOfPaymentTemplate()
        {
            return """
                <!DOCTYPE html>
                <html lang="en">
                  <head>
                    <meta charset="UTF-8" />
                    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                    <title>Invoice</title>
                    <style>
                      html {
                        font-family: Verdana, Geneva, Tahoma, sans-serif;
                        margin: 0;
                        padding: 0;
                        font-size: 12px;
                        display: flex;
                        justify-content: center;
                        flex-direction: column;
                      }

                      table {
                        width: 100%;
                      }
                      body {
                        margin: 5px;
                        padding: 0;
                      }

                      table tr td:nth-child(2) {
                        text-align: right;
                      }
                      .qr-code {
                        text-align: center;
                        margin-top: 20px;
                      }

                      /* invoice header */
                      .invoce-header {
                        text-align: center;
                      }
                      .mb-9 {
                        margin-bottom: 9px;
                      }

                      .receit-title {
                        font-size: 20px;
                        font-weight: bold;
                        align-items: center;
                      }

                      .bold {
                        font-weight: bold;
                      }

                      .m0 {
                        margin: 0;
                      }
                      .p0 {
                        padding: 0;
                      }

                      /* details */
                      table.details th,
                      td {
                        margin: 0 !important;
                        padding: 0 !important;
                      }
                    </style>
                  </head>
                  <body>
                    <table>
                      <tr>
                        <td class="title">
                          <center>
                            <img
                              src="{Logo}"
                              style="width: 200px"
                            />
                          </center>
                        </td>
                      </tr>
                      <tr>
                        <td class="invoce-header">
                          <div class="mb-9 receit-title">{Company}</div>
                          <div class="mb-9">{Document}</div>
                          <div class="mb-9">{Tel}</div>
                          <div class="mb-9">{Mail}</div>
                          <div>{Address}</div>
                          <div>------------------------------------------------------</div>
                          <b class="bold">RECIBO DE CUENTA POR COBRAR</b>
                          <div>{Date}</div>
                          <div>------------------------------------------------------</div>
                        </td>
                      </tr>
                    </table>
                    <table class="details">
                      <tr>
                        <th>Recibí de:</th>
                        <td>{Client}</td>
                      </tr>
                      <tr>
                        <td colspan="2">
                          <center>
                            ------------------------------------------------------
                          </center>
                        </td>
                      </tr>
                      <tr>
                      <th>Fecha</th>
                      <td>{PaymentDate}</td>
                    </tr>
                    <tr>
                      <th>Monto</th>
                      <td>{Paid}</td>
                    </tr>
                    <tr>
                      <th>Pendiente:</th>
                      <td>{Pending}</td>
                    </tr>
                    </table>
                    <div>
                      <br />
                      <br />
                      <div class="bold">Concepto:</div>
                      <div style="margin-left: 20px">PAGO DE CUENTAS POR COBRAR</div>

                      <br />
                      <br />
                      <br />
                      <br />
                      <div style="border-top: solid 1px black; text-align: center">
                        {Signer}
                      </div>
                    </div>
                  </body>
                </html>
                
                """;
        }

        public static string GetUserInvitationEmailTemplate()
        {
            return """
                <html>
                  <head>
                    <style>
                      body {
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f4f4f4;
                      }
                      .container {
                        width: 100%;
                        max-width: 600px;
                        margin: 0 auto;
                        background-color: #ffffff;
                        padding: 20px;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                      }
                      .header {
                        text-align: center;
                        padding: 20px 0;
                      }
                      .header img {
                        max-width: 150px;
                      }
                      .content {
                        text-align: center;
                        padding: 20px 0;
                      }
                      .content a {
                        display: inline-block;
                        padding: 10px 20px;
                        background-color: #007bff;
                        color: #ffffff;
                        text-decoration: none;
                        border-radius: 5px;
                      }
                      .content a:hover {
                        background-color: #0056b3;
                      }
                    </style>
                  </head>
                  <body>
                    <div class="container">
                      <div class="header">
                        <img
                          src="{Logo}"
                          alt="Company Logo"
                        />
                      </div>
                      <div class="content">
                        <h1>Invitación al sistema GPA</h1>
                        <p>
                          <b>Válida por 24 horas</b>. Para completar el proceso debe hacer click en <b>Aceptar invitación</b>
                        </p>
                        <a href="http://localhost:4200/auth/invitation-redemption/{Token}">Aceptar invitación</a>
                      </div>
                    </div>
                  </body>
                </html>
                """;
        }

        public static string GetPasswordResetTemplate()
        {
            return """
                <html>
                  <head>
                    <style>
                      body {
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f4f4f4;
                      }
                      .container {
                        width: 100%;
                        max-width: 600px;
                        margin: 0 auto;
                        background-color: #ffffff;
                        padding: 20px;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                      }
                      .header {
                        text-align: center;
                        padding: 20px 0;
                      }
                      .header img {
                        max-width: 150px;
                      }
                      .content {
                        text-align: center;
                        padding: 20px 0;
                      }
                      .content a {
                        display: inline-block;
                        padding: 10px 20px;
                        background-color: #007bff;
                        color: #ffffff;
                        text-decoration: none;
                        border-radius: 5px;
                      }
                      .content a:hover {
                        background-color: #0056b3;
                      }
                    </style>
                  </head>
                  <body>
                    <div class="container">
                      <div class="header">
                        <img
                          src="{Logo}"
                          alt="Company Logo"
                        />
                      </div>
                      <div class="content">
                        <h1>Código TOTP</h1>
                        <p>
                          Código de reestablecimiento de contraseña. Por favor, no comparta este código con nadie. El código es válio por 9 minutos: {Code}
                        </p>
                      </div>
                    </div>
                  </body>
                </html>                   
                """;
        }

        public static string GetInvitationRedemptionTemplate()
        {
            return """
                <html>
                  <head>
                    <style>
                      body {
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f4f4f4;
                      }
                      .container {
                        width: 100%;
                        max-width: 600px;
                        margin: 0 auto;
                        background-color: #ffffff;
                        padding: 20px;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                      }
                      .header {
                        text-align: center;
                        padding: 20px 0;
                      }
                      .header img {
                        max-width: 150px;
                      }
                      .content {
                        text-align: center;
                        padding: 20px 0;
                      }
                      .content a {
                        display: inline-block;
                        padding: 10px 20px;
                        background-color: #007bff;
                        color: #ffffff;
                        text-decoration: none;
                        border-radius: 5px;
                      }
                      .content a:hover {
                        background-color: #0056b3;
                      }
                    </style>
                  </head>
                  <body>
                    <div class="container">
                      <div class="header">
                        <img
                          src="{Logo}"
                          alt="Company Logo"
                        />
                      </div>
                      <div class="content">
                        <h1>Validando invitación al sistema GPA</h1>
                        <p>
                          El siguiente código es para completar el proceso de invitación. El código es <b>válido por 9 minutos</b>.
                        </p>
                        <br/>
                        Código: {Code}
                        <br/>
                        Usuario: {User}
                      </div>
                    </div>
                  </body>
                </html>
                """;
        }

        public static string TRANSACTION_TEMPLATE = "TRANSACTION_TEMPLATE";
        public static string STOCK_CYCLE_DETAILS_TEMPLATE = "STOCK_CYCLE_DETAILS_TEMPLATE";
        public static string SALE_TEMPLATE = "SALE_TEMPLATE";
        public static string INVOICE_TEMPLATE = "INVOICE_TEMPLATE";
        public static string PROOF_OF_PAYMENT_TEMPLATE = "PROOF_OF_PAYMENT_TEMPLATE";
        public static string RECEIVABLE_PROOF_OF_PAYMENT_TEMPLATE = "RECEIVABLE_PROOF_OF_PAYMENT_TEMPLATE";
        public static string EXISTENCE_TEMPLATE = "EXISTENCE_TEMPLATE";
        public static string USER_INVITATION_TEMPLATE = "USER_INVITATION_TEMPLATE";
        public static string PASSWORD_RESET_TEMPLATE = "PASSWORD_RESET_TEMPLATE";
        public static string USER_INVITATION_REDEMPTION_TEMPLATE = "USER_INVITATION_REDEMPTION_TEMPLATE";
    }
}
