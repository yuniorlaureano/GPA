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
    <h1 style="text-align: center">Ventas</h1>
    <table>
      <tr class="header">
        <th style="width: 70px">Estado</th>
        <th>Código</th>
        <th style="width: 80">Tipo</th>
        <th style="width: 60">Fecha</th>
        <th>Nota</th>
        <th>Cliente</th>
        <th>Estado de pago</th>
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
                      <div class="bold">Concento:</div>
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
                      <div class="bold">Concento:</div>
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

        public static string TRANSACTION_TEMPLATE = "TRANSACTION_TEMPLATE";
        public static string STOCK_CYCLE_DETAILS_TEMPLATE = "STOCK_CYCLE_DETAILS_TEMPLATE";
        public static string SALE_TEMPLATE = "SALE_TEMPLATE";
        public static string INVOICE_TEMPLATE = "INVOICE_TEMPATE";
        public static string PROOF_OF_PAYMENT_TEMPLATE = "PROOF_OF_PAYMENT_TEMPLATE";
        public static string RECEIVABLE_PROOF_OF_PAYMENT_TEMPLATE = "RECEIVABLE_PROOF_OF_PAYMENT_TEMPLATE";
    }
}
