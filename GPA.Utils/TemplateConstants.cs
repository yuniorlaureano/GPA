namespace GPA.Utils
{
    public class TemplateConstants
    {
        public static string TransactionTemplate()
        {
            return """ 
<html lang="en">
  <head>
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

        public static string TRANSACTION_TEMPLATE = "TRANSACTION_TEMPLATE";
        public static string STOCK_DETAILS_TEMPLATE = "STOCK_DETAILS_TEMPLATE";
        public static string SALE_TEMPLATE = "SALE_TEMPLATE";
    }
}
