﻿using GPA.Common.Entities.Inventory;
using GPA.Entities;
using GPA.Entities.General;

namespace GPA.Common.Entities.Invoice
{
    public class Invoice : Entity<Guid>
    {
        public string Code { get; set; }
        public SaleType Type { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal Payment { get; set; }
        public decimal ToPay { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
        public Guid ClientId { get; set; }
        public required Client Client { get; set; }
        
        public ICollection<Stock> Stocks { get; set; }
        public required ICollection<InvoiceDetails> InvoiceDetails { get; set; }
        public ICollection<InvoiceDelivery>? InvoiceDeliveries { get; set; }
        public ICollection<ClientPaymentsDetails>? ClientPaymentsDetails { get; set; }
        public ICollection<InvoiceAttachment> InvoiceAttachments { get; set; }
    }
}
